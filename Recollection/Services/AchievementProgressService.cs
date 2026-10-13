using MethodTimer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Recollection.Services;

public class AchievementProgressService : IDisposable {
    private readonly UnlockablesService unlockablesService;
    private readonly DatabaseService databaseService;
    private readonly ConcurrentDictionary<uint, uint> progressCache = new();
    private bool updated = false;

    [Time]
    public AchievementProgressService(Plugin plugin) {
        unlockablesService = plugin.UnlockablesService;
        databaseService = plugin.DatabaseService;
        SetupEvent();
        LoadProgress();
    }

    private unsafe void SetupEvent() {
        Plugin.ReceiveAchievementProgressHook.OnDetour += (_, id, current, _) => SetProgress(id, current);
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
        var all = databaseService.GetAllAchievementStatus(Plugin.PlayerState.ContentId);
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
        var lastId = unlockablesService.HighestAchievementIdMap.GetValueOrDefault(achievementId, achievementId);

        progressCache[lastId] = progress;
        unlockablesService.SetAchievementProgress(lastId, progress);
    }

    public uint? IncrementProgress(uint achievementId, int amount) {
        var lastId = unlockablesService.HighestAchievementIdMap.GetValueOrDefault(achievementId, achievementId);

        if (progressCache.TryGetValue(lastId, out var current)) {
            current += (uint) amount;
            progressCache[lastId] = current;
            unlockablesService.SetAchievementProgress(lastId, current);
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
