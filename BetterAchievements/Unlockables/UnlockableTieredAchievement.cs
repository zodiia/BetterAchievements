using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Unlockables;

public record UnlockableTieredAchievement
{
    public UnlockableTieredAchievement(List<Achievement> achievements, string name)
    {
        Achievements = achievements;
        Name = name;
        UnlockableAchievements = Achievements.Select(it => new UnlockableAchievement(it)).ToList();
        Unlocked = Achievements.Select(it => Plugin.UnlockState.IsAchievementComplete(it)).ToList();
        NameLowercase = Name.ToLower();
        DescriptionLowercase = string.Join(" ", UnlockableAchievements.Select(it => it.Description().ToLower()).ToList());
    }

    public readonly List<Achievement> Achievements;
    public readonly string Name;

    internal List<UnlockableAchievement> UnlockableAchievements { get; }
    internal List<bool> Unlocked { get; }
    public int Count => Unlocked.Count;

    public readonly string NameLowercase;
    public readonly string DescriptionLowercase;
}
