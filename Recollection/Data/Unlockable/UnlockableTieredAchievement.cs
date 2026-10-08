using MethodTimer;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableTieredAchievement : IUnlockable {
    private readonly bool spoilers;

    [Time]
    public UnlockableTieredAchievement(List<Achievement> excelAchievements, List<UnlockableAchievement> providesAchievements, bool spoilers, Plugin plugin) {
        this.spoilers = spoilers;
        ExcelAchievements = excelAchievements;
        Spoilers = spoilers;
        ProvidesAchievements = providesAchievements;
        Description = excelAchievements.Last().Description.ToString();
        DescriptionLowercase = string.Join(" ", ProvidesAchievements.Select(it => it.Description.ToLower()).ToList());
        Maximum = (uint)ProvidesAchievements.Count;
        MaximumPoints = (uint)ExcelAchievements.Select(it => (int)it.Points).Sum();
        Ids = ExcelAchievements.Select(it => it.RowId).ToList();
        Pinned = ExcelAchievements.Any(it => plugin.Configuration.PinnedAchievements.Contains(it.RowId));
        Name = "";
        NameLowercase = "";
        UpdateCurrent();
    }

    public uint Id => ExcelAchievements.Last().RowId;
    public UnlockableType Type => UnlockableType.Achievement;
    public uint Icon => ProvidesAchievements.Last().Icon;

    public string Name { get; private set; }
    public string Description { get; }
    public string? HowTo => null;
    public string NameLowercase { get; private set; }
    public string DescriptionLowercase { get; }
    public string? HowToLowercase => null;

    public uint? Current { get; set; }
    public uint Maximum { get; }
    public bool Unlocked { get; set; }

    public List<Achievement> ExcelAchievements { get; }
    public List<UnlockableAchievement> ProvidesAchievements { get; }
    public List<uint> Ids { get; }
    public bool Pinned { get; }
    public bool Spoilers { get; }
    public uint CurrentPoints { get; set; }
    public uint MaximumPoints { get; }

    public bool IsValid() => ProvidesAchievements.All(it => it.IsValid());

    public AchievementCompletionRatio AchievementCompletionRatio() =>
        (ProvidesAchievements.Find(it => !it.Unlocked) ?? ProvidesAchievements.Last()).AchievementCompletionRatio();

    public void UpdateCurrent() {
        Current = (uint)ProvidesAchievements.Count(it => it.Unlocked);
        CurrentPoints = (uint)ProvidesAchievements.Where(it => it.Unlocked).Sum(it => it.Points);
        Name = spoilers switch {
            false => CompiledRegexes.AchievementNameReplace().Replace(ProvidesAchievements.Last().Name, ""),
            true => CompiledRegexes.AchievementNameReplace()
                                   .Replace((ProvidesAchievements.FindLast(it => it.Unlocked) ?? ProvidesAchievements.First()).Name, "")
        };
        NameLowercase = Name.ToLower();
        Unlocked = Current == Maximum;
    }
}
