using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Serilog;

namespace BetterAchievements.Data;

public record MainLayout
{
    private const string AchievementCategoryLegacy = "Legacy";

    public required List<AchievementLayout> AchievementLayout { get; set; }

    private static bool IsExcelAchievementInvalid(Achievement ach)
    {
        return ach.Name.IsEmpty
               || !ach.AchievementCategory.IsValid || ach.AchievementCategory.Value.Name.IsEmpty
               || !ach.AchievementCategory.Value.AchievementKind.IsValid || ach.AchievementCategory.Value.AchievementKind.Value.Name.IsEmpty
               || ach.AchievementCategory.Value.AchievementKind.Value.Name.ToString().Equals(AchievementCategoryLegacy);
    }

    public void CheckMissingAchievements(ExcelSheet<Achievement> excel)
    {
        var achievements = AchievementLayout.SelectMany(it => it.GetAllAchievementIds()).ToList();

        foreach (var ach in excel)
        {
            if (IsExcelAchievementInvalid(ach))
            {
                continue;
            }

            if (!achievements.Contains(ach.RowId))
            {
                Log.Warning("Achievement #{Id}, \"{Name}: {Desc}\", in {Category}/{Subcategory}, is not currently mapped",
                            ach.RowId, ach.Name.ToString(), ach.Description.ToString(),
                            ach.AchievementCategory.Value.AchievementKind.Value.Name.ToString(),
                            ach.AchievementCategory.Value.Name.ToString());
            }
        }
        foreach (var id in achievements)
        {
            if (!excel.HasRow(id))
            {
                Log.Warning("Layout contains achievement #{Id} which doesn't seem to exist", id);
            }

            var ach = excel[id];

            if (IsExcelAchievementInvalid(ach))
            {
                Log.Warning("Layout contains achievement #{Id} which is invalid", id);
            }
        }
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutGroup), typeDiscriminator: "group")]
[JsonDerivedType(typeof(AchievementLayoutCategory), typeDiscriminator: "category")]
public abstract record AchievementLayout
{
    public required string Name { get; init; }

    public abstract List<uint> GetAllAchievementIds();
}

public record AchievementLayoutGroup : AchievementLayout
{
    public required List<AchievementLayout> Items { get; init; }

    public override List<uint> GetAllAchievementIds() => Items.SelectMany(it => it.GetAllAchievementIds()).ToList();
}

public record AchievementLayoutCategory : AchievementLayout
{
    public required List<AchievementLayoutItem> Items { get; init; }
    public required int Id { get; init; }

    public override List<uint> GetAllAchievementIds()
    {
        return Items.SelectMany(it => it switch
        {
            AchievementLayoutItemSimple simple => [simple.Id],
            AchievementLayoutItemTiered tiered => tiered.Ids,
            AchievementLayoutItemCombined combined => combined.Ids,
            _ => throw new ArgumentOutOfRangeException(nameof(it), it, null)
        }).ToList();
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutItemSimple), typeDiscriminator: "simple")]
[JsonDerivedType(typeof(AchievementLayoutItemTiered), typeDiscriminator: "tiered")]
[JsonDerivedType(typeof(AchievementLayoutItemCombined), typeDiscriminator: "combined")]
public abstract record AchievementLayoutItem { }

public record AchievementLayoutItemSimple : AchievementLayoutItem
{
    public required uint Id { get; init; }
}

public record AchievementLayoutItemTiered : AchievementLayoutItem
{
    public required List<uint> Ids { get; init; }
    public bool Spoilers { get; init; } = false;
}

public record AchievementLayoutItemCombined : AchievementLayoutItem
{
    public required List<uint> Ids { get; init; }
}
