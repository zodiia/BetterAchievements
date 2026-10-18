using MethodTimer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Recollection.Data;
using Recollection.Data.Unlockable;
using Recollection.Helpers;
using Recollection.Services;

namespace Recollection.UI.State;

public sealed record AchievementRoulette(
    Services.AchievementRoulette Roulette,
    UnlockableAchievement Progress,
    string Breadcrumb,
    uint ProgressAtRoll,
    uint[] Daily) {
    public Achievement Achievement { get; } = ExcelSheets.Achievement.Value.GetRow(Roulette.AchievementId);
    public DateTime RolledAt { get; } = ToLocalDateTime(Roulette.RollDate);
    public DateTime? CompletedAt { get; } = Roulette.CompletionDate is { } completionDate ? ToLocalDateTime(completionDate) : null;

    public uint Maximum => Achievement.Maximum();
    public string RolledAtText => RolledAt.ToString(RouletteState.LongDateFormat, CultureInfo.InvariantCulture);
    public string? CompletedAtText => CompletedAt?.ToString(RouletteState.LongDateFormat, CultureInfo.InvariantCulture);
    public double PerDay => Daily.Sum(it => (double)it) / Math.Max(1.0, ((CompletedAt ?? DateTime.Now) - RolledAt).TotalDays);
    public uint Current => CompletedAt != null ? Maximum : Math.Min(Progress.Current ?? ProgressAtRoll, Maximum);

    internal static DateTime ToLocalDateTime(long timestamp) => DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
}

public readonly record struct AchievementPoolEntry(uint Id, string Breadcrumb, bool NextTier, bool HasCounter, byte Points);

public sealed class RouletteState : IDisposable {
    public const string LongDateFormat = "dd MMMM yyyy 'at' HH:mm";

    private readonly Configuration configuration;
    private readonly MainLayout mainLayout;
    private readonly UnlockablesService unlockablesService;
    private readonly DatabaseService databaseService;
    private readonly AchievementRouletteService rouletteService;

    private List<AchievementPoolEntry> achievementPool = [];

    public List<AchievementRoulette> Active { get; private set; } = [];
    public List<AchievementRoulette> Completed { get; private set; } = [];

    public List<AchievementPoolEntry> RouletteCandidates { get; private set; } = [];
    public Dictionary<string, int> CategoryCounts { get; private set; } = new();

    public RouletteState(Plugin plugin) {
        configuration = plugin.Configuration;
        mainLayout = plugin.MainLayout;
        unlockablesService = plugin.UnlockablesService;
        databaseService = plugin.DatabaseService;
        rouletteService = plugin.AchievementRouletteService;

        rouletteService.OnRouletteChanged += Rebuild;
        rouletteService.OnRouletteProgress += OnRouletteProgress;
        Plugin.UnlockState.Unlock += OnUnlock;
        Rebuild();
    }

    public void Dispose() {
        rouletteService.OnRouletteChanged -= Rebuild;
        rouletteService.OnRouletteProgress -= OnRouletteProgress;
        Plugin.UnlockState.Unlock -= OnUnlock;
    }

    public void Roll(uint achievementId) => rouletteService.Roll(achievementId);

    public void Abandon(AchievementRoulette challenge) => rouletteService.Abandon(challenge.Roulette);

    public void UpdateSettings(Action<Configuration> update) {
        update(configuration);
        configuration.Save();
        ApplySettings();
    }

    [Time]
    public void Rebuild() {
        var challenges = rouletteService.GetActiveRoulette().Select(BuildChallenge).ToList();
        Active = challenges.Where(it => it.CompletedAt == null).ToList();
        Completed = challenges.Where(it => it.CompletedAt != null).OrderByDescending(it => it.CompletedAt).ToList();

        BuildPool();
    }

    private void OnRouletteProgress(uint achievementId) {
        var highestId = rouletteService.HighestAchievementId(achievementId);
        for (var i = 0; i < Active.Count; i++) {
            if (rouletteService.HighestAchievementId(Active[i].Roulette.AchievementId) == highestId) {
                Active[i] = BuildChallenge(Active[i].Roulette);
            }
        }
    }

    private void OnUnlock(RowRef rowRef) {
        if (rowRef.Is<Achievement>()) {
            BuildPool();
        }
    }

    private List<AchievementUpdate> GetUpdates(Services.AchievementRoulette roulette) =>
        databaseService.GetAchievementUpdates(rouletteService.AchievementSeries(roulette.AchievementId), roulette.CharacterId)
                       .Where(it => it.Progress != null)
                       .ToList();

    private UnlockableAchievement GetUnlockableAchievement(Services.AchievementRoulette roulette) =>
        unlockablesService.GetUnlockableAchievement(rouletteService.HighestAchievementId(roulette.AchievementId));

    private static uint GetProgressOnRoll(List<AchievementUpdate> updates, long rollDate, uint maximum) {
        var updateAtRoll = updates.LastOrDefault(it => it.Timestamp <= rollDate) ?? updates.FirstOrDefault();
        return Math.Min(updateAtRoll?.Progress ?? 0, maximum);
    }

    private static uint? GetProgressAtEndOfDay(List<AchievementUpdate> updates, DateTime day, uint maximum) {
        var dayEnd = new DateTimeOffset(day.AddDays(1)).ToUnixTimeSeconds();
        return updates.LastOrDefault(it => it.Timestamp < dayEnd)?.Progress is { } progress ? Math.Min(progress, maximum) : null;
    }

    private static uint[] GetDailyProgress(List<AchievementUpdate> updates, Services.AchievementRoulette roulette, uint progressAtRoll, uint maximum) {
        var rolledAt = AchievementRoulette.ToLocalDateTime(roulette.RollDate);
        var end = roulette.CompletionDate is { } completionDate ? AchievementRoulette.ToLocalDateTime(completionDate) : DateTime.Now;
        var daily = new uint[Math.Max(1, (end.Date - rolledAt.Date).Days + 1)];
        var previous = progressAtRoll;

        for (var day = 0; day < daily.Length; day++) {
            var value = GetProgressAtEndOfDay(updates, rolledAt.Date.AddDays(day), maximum) ?? previous;
            daily[day] = value > previous ? value - previous : 0;
            previous = Math.Max(previous, value);
        }

        return daily;
    }

    [Time]
    private AchievementRoulette BuildChallenge(Services.AchievementRoulette roulette) {
        var maximum = ExcelSheets.Achievement.Value.GetRow(roulette.AchievementId).Maximum();
        var updates = GetUpdates(roulette);
        var progressAtRoll = GetProgressOnRoll(updates, roulette.RollDate, maximum);

        return new AchievementRoulette(roulette, GetUnlockableAchievement(roulette), FindBreadcrumb(roulette.AchievementId), progressAtRoll,
                                       GetDailyProgress(updates, roulette, progressAtRoll, maximum));
    }

    private IEnumerable<(AchievementLayoutCategory Category, string Breadcrumb)> Categories() =>
        mainLayout.Achievements.SelectMany(group => group.Items.Select(category => (category, $"{group.Name} / {category.Name}")));

    private string FindBreadcrumb(uint achievementId) =>
        Categories().Where(it => it.Category.GetAllAchievementIds().Contains(achievementId)).Select(it => it.Breadcrumb).FirstOrDefault("Unknown");

    [Time]
    private void BuildPool() {
        var active = rouletteService.GetActiveRoulette().Select(it => it.AchievementId).ToHashSet();

        achievementPool = Categories().SelectMany(it => it.Category.Sections.Where(section => !section.Seasonal)
                                                          .SelectMany(section => section.Items)
                                                          .SelectMany(item => PoolEntries(item, it.Breadcrumb)))
                                      .Where(it => !active.Contains(it.Id))
                                      .ToList();
        ApplySettings();
    }

    private static IEnumerable<AchievementPoolEntry> PoolEntries(AchievementLayoutItem item, string breadcrumb) {
        var ids = item switch {
            AchievementLayoutItemSimple simple => [simple.Id],
            AchievementLayoutItemTiered tiered => tiered.Ids,
            _ => [],
        };

        return ids.Select(ExcelSheets.Achievement.Value.GetRowOrDefault)
                  .OfType<Achievement>()
                  .Where(it => it.IsValidEntry() && !Plugin.UnlockState.IsAchievementComplete(it))
                  .Select((it, index) => new AchievementPoolEntry(it.RowId, breadcrumb, index == 0, it.Maximum() > 1, it.Points));
    }

    private void ApplySettings() {
        var counts = new Dictionary<string, int>();
        var candidates = new List<AchievementPoolEntry>();

        foreach (var entry in achievementPool) {
            if (entry.Points < configuration.RouletteMinPoints || entry.Points > configuration.RouletteMaxPoints) {
                continue;
            }

            if (configuration.RouletteNextTierOnly && !entry.NextTier) continue;
            if (configuration.RouletteOnlyWithCounter && !entry.HasCounter) continue;

            counts[entry.Breadcrumb] = counts.GetValueOrDefault(entry.Breadcrumb) + 1;
            if (!configuration.RouletteDisabledCategories.Contains(entry.Breadcrumb)) {
                candidates.Add(entry);
            }
        }

        CategoryCounts = counts;
        RouletteCandidates = candidates;
    }
}
