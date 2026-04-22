using System.Collections.Concurrent;
using BetterAchievements.UI.Windows;

namespace BetterAchievements.Services;

public class UnlockablesProgressService
{
    private readonly ConcurrentDictionary<uint, uint> progressCache = new();
    private MainWindowState? stateToRefresh;

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
        stateToRefresh?.Refresh();
    }

    public uint? IncrementProgress(uint achievementId, int amount)
    {
        if (progressCache.TryGetValue(achievementId, out var current))
        {
            current = (uint) (current + amount); // man i hate c#
            progressCache[achievementId] = current;
            stateToRefresh?.Refresh();
            return current;
        }

        return null;
    }

    public void SetMainWindowStateToRefresh(MainWindowState state)
    {
        stateToRefresh = state;
    }
}
