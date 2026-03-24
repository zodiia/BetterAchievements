using System.Collections.Concurrent;

namespace BetterAchievements.Unlockables;

public class UnlockablesProgressService
{
    private readonly ConcurrentDictionary<uint, uint> progressCache = new();

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
    }

    public uint? IncrementProgress(uint achievementId, int amount)
    {
        if (progressCache.TryGetValue(achievementId, out var current))
        {
            current = (uint) (current + amount); // man i hate c#
            progressCache[achievementId] = current;
            return current;
        }

        return null;
    }
}
