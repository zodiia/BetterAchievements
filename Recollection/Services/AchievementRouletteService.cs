using MethodTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Recollection.Helpers;
using Action = System.Action;

namespace Recollection.Services;

public sealed class AchievementRouletteService : IDisposable {
    private readonly IPluginLog log = Plugin.GetLogger<AchievementRouletteService>();
    private readonly DatabaseService databaseService;
    private readonly AchievementProgressService achievementProgressService;
    private readonly Dictionary<uint, uint> highestAchievementIdMap;
    private bool updated;

    private List<AchievementRoulette> activeRoulette = [];

    public event Action? OnRouletteChanged;
    public event Action<uint>? OnRouletteProgress;

    [Time]
    public AchievementRouletteService(Plugin plugin) {
        databaseService = plugin.DatabaseService;
        achievementProgressService = plugin.AchievementProgressService;
        highestAchievementIdMap = plugin.UnlockablesService.HighestAchievementIdMap;
        LoadRoulette();
        SetupEvents();
    }

    private void SetupEvents() {
        Plugin.UnlockState.Unlock += OnUnlock;
        achievementProgressService.OnProgressReceived += OnProgressReceived;
        Plugin.Framework.Update += OnFrameworkUpdate;
        Plugin.ClientState.Login += OnLogin;
    }

    public void Dispose() {
        Plugin.UnlockState.Unlock -= OnUnlock;
        achievementProgressService.OnProgressReceived -= OnProgressReceived;
        Plugin.Framework.Update -= OnFrameworkUpdate;
        Plugin.ClientState.Login -= OnLogin;
    }

    private void LoadRoulette() {
        activeRoulette = databaseService.GetAchievementRoulettes(Plugin.PlayerState.ContentId);
        OnRouletteChanged?.Invoke();
    }

    public uint HighestAchievementId(uint achievementId) =>
        highestAchievementIdMap.GetValueOrDefault(achievementId, achievementId);

    public List<uint> AchievementSeries(uint achievementId) =>
        highestAchievementIdMap.Where(it => it.Value == HighestAchievementId(achievementId))
                               .Select(it => it.Key)
                               .Append(achievementId)
                               .Distinct()
                               .ToList();


    public void Roll(uint achievementId) {
        var roulette = databaseService.InsertAchievementRoulette(Plugin.PlayerState.ContentId, achievementId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        activeRoulette.Insert(0, roulette);
        OnRouletteChanged?.Invoke();
    }

    public void Abandon(AchievementRoulette roulette) {
        databaseService.DeleteAchievementRoulette(roulette.Id);
        activeRoulette.RemoveAll(it => it.Id == roulette.Id);
        OnRouletteChanged?.Invoke();
    }

    private void Complete(AchievementRoulette roulette, long completionDate) {
        var idx = activeRoulette.FindIndex(it => it.Id == roulette.Id);

        databaseService.CompleteAchievementRoulette(roulette.Id, completionDate);
        activeRoulette[idx] = roulette with { Status = true, CompletionDate = completionDate };
    }

    public IReadOnlyList<AchievementRoulette> GetActiveRoulette() => activeRoulette;

    private void OnUnlock(RowRef rowRef) {
        if (!rowRef.Is<Achievement>()) return;

        var completed = activeRoulette.Where(it => !it.Status && it.AchievementId == rowRef.RowId).ToList();
        if (completed.Count == 0) return;

        completed.ForEach(it => Complete(it, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
        OnRouletteChanged?.Invoke();
    }

    private void OnProgressReceived(uint achievementId) {
        var highestId = HighestAchievementId(achievementId);
        if (activeRoulette.Any(it => !it.Status && HighestAchievementId(it.AchievementId) == highestId)) {
            OnRouletteProgress?.Invoke(achievementId);
        }
    }

    [Time]
    private void OnFrameworkUpdate(IFramework framework) {
        if (updated || !Plugin.UnlockState.IsAchievementListLoaded) {
            return;
        }

        var characterId = Plugin.PlayerState.ContentId;
        var completed = activeRoulette.Where(it => !it.Status)
                                      .Where(it => Plugin.UnlockState.IsAchievementComplete(ExcelSheets.Achievement.Value.GetRow(it.AchievementId)))
                                      .ToList();

        updated = true;
        Plugin.Framework.Update -= OnFrameworkUpdate;

        if (completed.Count == 0) {
            return;
        }

        foreach (var roulette in completed) {
            var unlock = databaseService.GetAchievementUpdates(roulette.AchievementId, characterId).Where(it => it.Status).MaxBy(it => it.Timestamp);
            Complete(roulette, unlock?.Timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        OnRouletteChanged?.Invoke();
    }

    private void OnLogin() {
        LoadRoulette();

        if (updated) {
            updated = false;
            Plugin.Framework.Update += OnFrameworkUpdate;
        }
    }
}
