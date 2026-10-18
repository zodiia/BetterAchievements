using MethodTimer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Recollection.Services;

public class AchievementProgressService : IDisposable {
    private readonly UnlockablesService unlockablesService;
    private readonly DatabaseService databaseService;
    private readonly ConcurrentDictionary<uint, uint> progressCache = new();
    private bool updated = false;

    public event Action<uint>? OnProgressReceived;

    [Time]
    public AchievementProgressService(Plugin plugin) {
        unlockablesService = plugin.UnlockablesService;
        databaseService = plugin.DatabaseService;
        SetupEvent();
        LoadProgress();
    }

    private unsafe void SetupEvent() {
        Plugin.ReceiveAchievementProgressHook.OnDetour += (_, id, current, max) => OnReceiveAchievementProgress(id, current, max);
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

    private void OnReceiveAchievementProgress(uint achievementId, uint current, uint max) {
        databaseService.UpdateAchievementStatus(new AchievementStatus {
            CharacterId = Plugin.PlayerState.ContentId, AchievementId = achievementId, Progress = current, Status = current == max
        });
        SetProgress(achievementId, current);
        OnProgressReceived?.Invoke(achievementId);
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
            current += (uint)amount;
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

    public static unsafe void OpenAchievementWindow(uint achievementId) {
        if (AgentModule.Instance() is null || AgentModule.Instance()->GetAgentAchievement() is null) {
            return;
        }

        AgentModule.Instance()->GetAgentAchievement()->OpenById(achievementId);
    }
}
