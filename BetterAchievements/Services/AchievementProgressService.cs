using System.Collections.Concurrent;

namespace BetterAchievements.Services;

public class AchievementProgressService {
    private readonly Plugin plugin;
    private readonly UnlockablesService unlockables;
    private readonly HistoryService history;
    private readonly ConcurrentDictionary<uint, uint> progressCache = new();
    private bool updated = false;

    public AchievementProgressService(Plugin plugin) {
        this.plugin = plugin;
        unlockables = plugin.UnlockablesService;
        history = plugin.HistoryService;
        SetupEvent();
        LoadProgress();
    }

    private unsafe void SetupEvent() {
        plugin.ReceiveAchievementProgressHook.OnDetour += (_, id, current, _) => SetProgress(id, current);
    }

    private void LoadProgress() {
        var all = history.GetAllAchievementStatus();
        foreach (var status in all) {
            if (status.Progress != null) {
                progressCache[status.AchievementId] = (uint)status.Progress;
            }
        }
    }

    public uint? GetProgress(uint achievementId) {
        if (progressCache.TryGetValue(achievementId, out var result)) {
            return result;
        }

        return null;
    }

    public void SetProgress(uint achievementId, uint progress) {
        var lastId = unlockables.HighestIdMap[achievementId];

        progressCache[lastId] = progress;
        updated = true;
    }

    public uint? IncrementProgress(uint achievementId, int amount) {
        var lastId = unlockables.HighestIdMap[achievementId];

        if (progressCache.TryGetValue(lastId, out var current)) {
            current += (uint) amount;
            progressCache[lastId] = current;
            updated = true;
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
