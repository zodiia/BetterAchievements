using MethodTimer;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableTieredAchievement : IUnlockable {
    [Time]
    public UnlockableTieredAchievement(List<Achievement> excelAchievements, bool spoilers, Plugin plugin) {
        ExcelAchievements = excelAchievements;
        Spoilers = spoilers;
        ProvidesAchievements = ExcelAchievements.Select(it => new UnlockableAchievement(it, plugin)).ToList();
        Name = spoilers switch {
            false => CompiledRegexes.AchievementNameReplace().Replace(ProvidesAchievements.Last().Name, ""),
            true => CompiledRegexes.AchievementNameReplace()
                                   .Replace((ProvidesAchievements.FindLast(it => it.Unlocked) ?? ProvidesAchievements.First()).Name, "")
        };
        Description = excelAchievements.Last().Description.ToString();
        NameLowercase = Name.ToLower();
        DescriptionLowercase = string.Join(" ", ProvidesAchievements.Select(it => it.Description.ToLower()).ToList());
        Current = (uint)ProvidesAchievements.Count(it => it.Unlocked);
        Maximum = (uint)ProvidesAchievements.Count;
        Unlocked = Current == Maximum;
        Ids = ExcelAchievements.Select(it => it.RowId).ToList();
        Pinned = ExcelAchievements.Any(it => plugin.Configuration.PinnedAchievements.Contains(it.RowId));
        MaximumPoints = (uint)ExcelAchievements.Select(it => (int)it.Points).Sum();
        CurrentPoints = (uint)ExcelAchievements.Select(it => Plugin.UnlockState.IsAchievementComplete(it) ? it.Points : 0).Sum();
    }

    public uint Id => ExcelAchievements.Last().RowId;
    public UnlockableType Type => UnlockableType.Achievement;
    public uint Icon => ProvidesAchievements.Last().Icon;

    public string Name { get; }
    public string Description { get; }
    public string? HowTo => null;
    public string NameLowercase { get; }
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
    public uint CurrentPoints { get; }
    public uint MaximumPoints { get; }

    public bool IsValid() => ProvidesAchievements.All(it => it.IsValid());

    public AchievementCompletionRatio AchievementCompletionRatio() =>
        (ProvidesAchievements.Find(it => !it.Unlocked) ?? ProvidesAchievements.Last()).AchievementCompletionRatio();
}
