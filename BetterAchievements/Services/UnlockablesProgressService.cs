using System.Collections.Concurrent;

namespace BetterAchievements.Services;

public class UnlockablesProgressService {
    private readonly Plugin plugin;
    private readonly UnlockablesService unlockables;
    private readonly ConcurrentDictionary<uint, uint> progressCache = new();
    private bool updated = false;

    public UnlockablesProgressService(Plugin plugin, UnlockablesService unlockables) {
        this.plugin = plugin;
        this.unlockables = unlockables;
        SetupEvent();
    }

    private unsafe void SetupEvent() {
        plugin.ReceiveAchievementProgressHook.OnDetour += (_, id, current, _) => SetProgress(id, current);
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
