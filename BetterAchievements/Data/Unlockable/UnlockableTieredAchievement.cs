using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Helpers;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public record UnlockableTieredAchievement : IUnlockable {
    private readonly bool spoilers;
    private readonly string name;
    private readonly string description;
    private readonly string nameLowercase;
    private readonly string descriptionLowercase;
    private readonly List<uint> ids;
    private readonly uint current;
    private readonly uint currentPoints;
    private readonly uint maximumPoints;
    private readonly bool pinned;
    private readonly List<UnlockableAchievement> providesAchievements;
    public readonly List<Achievement> ExcelAchievements;

    public UnlockableTieredAchievement(List<Achievement> excelAchievements, bool spoilers, Plugin plugin) {
        ExcelAchievements = excelAchievements;
        providesAchievements = ExcelAchievements.Select(it => new UnlockableAchievement(it, plugin)).ToList();
        name = spoilers switch {
            false => CompiledRegexes.AchievementNameReplace().Replace(providesAchievements.Last().Name(), ""),
            true => CompiledRegexes.AchievementNameReplace()
                                   .Replace((providesAchievements.FindLast(it => it.Unlocked()) ?? providesAchievements.First()).Name(), "")
        };
        this.spoilers = spoilers;
        description = excelAchievements.Last().Description.ToString();
        nameLowercase = name.ToLower();
        descriptionLowercase = string.Join(" ", providesAchievements.Select(it => it.Description().ToLower()).ToList());
        maximumPoints = (uint)ExcelAchievements.Select(it => (int)it.Points).Sum();
        currentPoints = (uint)ExcelAchievements.Select(it => Plugin.UnlockState.IsAchievementComplete(it) ? it.Points : 0).Sum();
        current = (uint)providesAchievements.Count(it => it.Unlocked());
        pinned = ExcelAchievements.Any(it => plugin.Configuration.PinnedAchievements.Contains(it.RowId));
        ids = ExcelAchievements.Select(it => it.RowId).ToList();
    }

    public uint Id() => ExcelAchievements.Last().RowId;
    public List<uint> Ids() => ids;
    public UnlockableType Type() => UnlockableType.Achievement;
    public uint Icon() => providesAchievements.Last().Icon();
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string HowTo() => "";
    public string HowToLowercase() => "";
    public uint? Current() => current;
    public uint Maximum() => (uint)providesAchievements.Count;
    public bool Unlocked() => Current() == Maximum();
    public bool Pinned() => pinned;
    public bool Spoilers() => spoilers;
    public uint CurrentPoints() => currentPoints;
    public uint MaximumPoints() => maximumPoints;
    public List<UnlockableAchievement> ProvidesAchievements() => providesAchievements;
    public bool IsValid() => providesAchievements.All(it => it.IsValid());

    public AchievementCompletionRatio AchievementCompletionRatio() =>
        (providesAchievements.Find(it => !it.Unlocked()) ?? providesAchievements.Last()).AchievementCompletionRatio();
}
