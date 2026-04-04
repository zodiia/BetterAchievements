using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Unlockables;

public record UnlockableTieredAchievement : Unlockable
{
    public UnlockableTieredAchievement(List<Achievement> achievements, string name)
    {
        Achievements = achievements;
        Name = name;
        UnlockableAchievements = Achievements.Select(it => new UnlockableAchievement(it)).ToList();
        Unlocked = Achievements.Select(it => Plugin.UnlockState.IsAchievementComplete(it)).ToList();
        nameLowercase = Name.ToLower();
        descriptionLowercase = string.Join(" ", UnlockableAchievements.Select(it => it.Description().ToLower()).ToList());
        TotalPoints = Achievements.Select(it => (int) it.Points).Sum();
    }

    public readonly List<Achievement> Achievements;
    public readonly string Name;
    public readonly int TotalPoints;

    internal List<UnlockableAchievement> UnlockableAchievements { get; }
    internal List<bool> Unlocked { get; }
    public int Count => Unlocked.Count;
    public int AcquiredPoints => Achievements.Select(it => Plugin.UnlockState.IsAchievementComplete(it) ? it.Points : 0).Sum();

    private readonly string nameLowercase;
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
}
