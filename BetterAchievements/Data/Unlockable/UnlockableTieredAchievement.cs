using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Helpers;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public record UnlockableTieredAchievement : IUnlockable
{
    public UnlockableTieredAchievement(List<Achievement> excelAchievements, bool spoilers, Plugin plugin)
    {
        ExcelAchievements = excelAchievements;
        providesAchievements = ExcelAchievements.Select(it => new UnlockableAchievement(it, plugin)).ToList();
        name = spoilers switch
        {
            false => CompiledRegexes.AchievementNameReplace().Replace(providesAchievements.Last().Name(), ""),
            true => CompiledRegexes.AchievementNameReplace().Replace((providesAchievements.FindLast(it => it.Unlocked()) ?? providesAchievements.First()).Name(), "")
        };
        this.spoilers = spoilers;
        description = excelAchievements.Last().Description.ToString();
        nameLowercase = name.ToLower();
        descriptionLowercase = string.Join(" ", providesAchievements.Select(it => it.Description().ToLower()).ToList());
        maximumPoints = (uint) ExcelAchievements.Select(it => (int) it.Points).Sum();
        currentPoints = (uint) ExcelAchievements.Select(it => Plugin.UnlockState.IsAchievementComplete(it) ? it.Points : 0).Sum();
        current = (uint) providesAchievements.Count(it => it.Unlocked());
        pinned = ExcelAchievements.Any(it => plugin.Configuration.PinnedAchievements.Contains(it.RowId));
        ids = ExcelAchievements.Select(it => it.RowId).ToList();
    }

    public readonly List<Achievement> ExcelAchievements;

    private readonly bool spoilers;
    private readonly string name;
    private readonly string description;
    private readonly uint maximumPoints;
    private readonly uint currentPoints;
    private readonly List<UnlockableAchievement> providesAchievements;

    public uint Id() => ExcelAchievements.Last().RowId;
    private readonly List<uint> ids;
    public List<uint> Ids() => ids;
    public UnlockableType Type() => UnlockableType.Achievement;
    public string Name() => name;
    public string Description() => description;
    private readonly string nameLowercase;
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    private readonly uint current;
    public uint? Current() => current;
    public uint Maximum() => (uint) providesAchievements.Count;
    public bool Unlocked() => Current() == Maximum();
    private readonly bool pinned;
    public bool Pinned() => pinned;
    public bool Spoilers() => spoilers;

    public uint CurrentPoints() => currentPoints;
    public uint MaximumPoints() => maximumPoints;
    public List<UnlockableAchievement> ProvidesAchievements() => providesAchievements;
}
