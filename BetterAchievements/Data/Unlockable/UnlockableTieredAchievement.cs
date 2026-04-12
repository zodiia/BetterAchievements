using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public record UnlockableTieredAchievement : IUnlockable
{
    public UnlockableTieredAchievement(List<Achievement> excelAchievements, string name)
    {
        ExcelAchievements = excelAchievements;
        this.name = name;
        description = excelAchievements.Last().Description.ToString();
        providesAchievements = ExcelAchievements.Select(it => new UnlockableAchievement(it)).ToList();
        nameLowercase = name.ToLower();
        descriptionLowercase = string.Join(" ", providesAchievements.Select(it => it.Description().ToLower()).ToList());
        maximumPoints = (uint) ExcelAchievements.Select(it => (int) it.Points).Sum();
        currentPoints = (uint) ExcelAchievements.Select(it => Plugin.UnlockState.IsAchievementComplete(it) ? it.Points : 0).Sum();
        current = (uint) providesAchievements.Count(it => it.Unlocked());
    }

    public readonly List<Achievement> ExcelAchievements;

    private readonly string name;
    private readonly string description;
    private readonly uint maximumPoints;
    private readonly uint currentPoints;
    private readonly List<UnlockableAchievement> providesAchievements;

    public uint Id() => ExcelAchievements.Last().RowId;
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

    public uint CurrentPoints() => currentPoints;
    public uint MaximumPoints() => maximumPoints;
    public List<UnlockableAchievement> ProvidesAchievements() => providesAchievements;
}
