using System.Collections.Concurrent;

namespace BetterAchievements.Services;

public class UnlockablesProgressService
{
    private readonly Plugin plugin;
    private readonly ConcurrentDictionary<uint, uint> progressCache = new();
    private bool updated = false;

    public UnlockablesProgressService(Plugin plugin)
    {
        this.plugin = plugin;
        SetupEvent();
    }

    private unsafe void SetupEvent()
    {
        plugin.ReceiveAchievementProgressHook.OnDetour += (_, id, current, _) => SetProgress(id, current);
    }

    public uint? GetProgress(uint achievementId)
    {
        if (progressCache.TryGetValue(achievementId, out var result))
        {
            return result;
        }

        return null;
    }

    public void SetProgress(uint achievementId, uint progress)
    {
        progressCache[achievementId] = progress;
        updated = true;
    }

    public uint? IncrementProgress(uint achievementId, int amount)
    {
        if (progressCache.TryGetValue(achievementId, out var current))
        {
            current = (uint) (current + amount); // man i hate c#
            progressCache[achievementId] = current;
            updated = true;
            return current;
        }

        return null;
    }

    public bool CheckUpdated()
    {
        if (updated)
        {
            updated = false;
            return true;
        }

        return false;
    }
}
