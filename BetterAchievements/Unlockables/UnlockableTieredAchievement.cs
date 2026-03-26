using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Unlockables;

public record UnlockableTieredAchievement(List<Achievement> Achievements, string Name)
{
    internal List<UnlockableAchievement> UnlockableAchievements { get; } = Achievements.Select(it => new UnlockableAchievement(it)).ToList();
    internal List<bool> Unlocked { get; } = Achievements.Select(it => Plugin.UnlockState.IsAchievementComplete(it)).ToList();
    public int Count => Unlocked.Count;

    public readonly string NameLowercase = Name.ToLower();
}
