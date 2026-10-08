using MethodTimer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Recollection.Services;

public class AchievementProgressService : IDisposable {
    private readonly Plugin plugin;
    private readonly UnlockablesService unlockables;
    private readonly HistoryService history;
    private readonly ConcurrentDictionary<uint, uint> progressCache = new();
    private bool updated = false;

    [Time]
    public AchievementProgressService(Plugin plugin) {
        this.plugin = plugin;
        unlockables = plugin.UnlockablesService;
        history = plugin.HistoryService;
        SetupEvent();
        LoadProgress();
    }

    private unsafe void SetupEvent() {
        plugin.ReceiveAchievementProgressHook.OnDetour += (_, id, current, _) => SetProgress(id, current);
        Plugin.ClientState.Login += OnLogin;
        Plugin.ClientState.Logout += OnLogout;
    }

    public void Dispose() {
        Plugin.ClientState.Login -= OnLogin;
        Plugin.ClientState.Logout -= OnLogout;
    }

    private void OnLogin() {
        LoadProgress();
    }

    private void OnLogout(int type, int code) {
        progressCache.Clear();
        updated = true;
    }

    [Time]
    private void LoadProgress() {
        progressCache.Clear();
        var all = history.GetAllAchievementStatus(Plugin.PlayerState.ContentId);
        foreach (var status in all) {
            if (status.Progress != null) {
                progressCache[status.AchievementId] = (uint)status.Progress;
            }
        }

        updated = true;
    }

    public uint? GetProgress(uint achievementId) {
        if (progressCache.TryGetValue(achievementId, out var result)) {
            return result;
        }

        return null;
    }

    public void SetProgress(uint achievementId, uint progress) {
        var lastId = unlockables.HighestAchievementIdMap.GetValueOrDefault(achievementId, achievementId);

        progressCache[lastId] = progress;
        unlockables.SetAchievementProgress(lastId, progress);
    }

    public uint? IncrementProgress(uint achievementId, int amount) {
        var lastId = unlockables.HighestAchievementIdMap.GetValueOrDefault(achievementId, achievementId);

        if (progressCache.TryGetValue(lastId, out var current)) {
            current += (uint) amount;
            progressCache[lastId] = current;
            unlockables.SetAchievementProgress(lastId, current);
            return current;
        }

        return null;
    }

    public bool CheckUpdated() {
        if (updated) {
            updated = false;
            return true;
        }

        return false;
    }
}
