using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data;

public record MainLayout {
    private const string AchievementCategoryLegacy = "Legacy";
    public static readonly IPluginLog Log = Plugin.GetLogger<MainLayout>();

    public required List<AchievementLayout> Achievements { get; init; }
    public required List<LayoutCategory> Mounts { get; init; }
    public required List<LayoutCategory> Minions { get; init; }
    public required List<LayoutCategory> Bardings { get; init; }
    public required List<LayoutCategory> Hairstyles { get; init; }
    public required List<LayoutCategory> Facewears { get; init; }
    public required List<LayoutCategory> Emotes { get; init; }

    [JsonPropertyName("ttcards")]
    public required List<LayoutCategory> TripleTriadCards { get; init; }

    [JsonPropertyName("ttnpcs")]
    public required List<LayoutCategory> TripleTriadNpcs { get; init; }

    [JsonPropertyName("fashionaccessories")]
    public required List<LayoutCategory> FashionAccessories { get; init; }

    [JsonPropertyName("framerskits")]
    public required List<LayoutCategory> FramersKits { get; init; }

    private static bool IsAchievementInvalid(Achievement ach) {
        return ach.Name.IsEmpty
               || !ach.AchievementCategory.IsValid || ach.AchievementCategory.Value.Name.IsEmpty
               || !ach.AchievementCategory.Value.AchievementKind.IsValid || ach.AchievementCategory.Value.AchievementKind.Value.Name.IsEmpty
               || ach.AchievementCategory.Value.AchievementKind.Value.Name.ToString().Equals(AchievementCategoryLegacy);
    }

    private static void CheckMissingCollectionEntries<T>(
        string kind, List<LayoutCategory> categories, ExcelSheet<T> excel, Func<T, bool> isValid, Func<T, string> getName)
        where T : struct, IExcelRow<T> {
        var ids = categories.SelectMany(it => it.Items).ToHashSet();

        foreach (var row in excel) {
            if (!isValid(row) || ids.Contains(row.RowId)) {
                continue;
            }

            var name = getName(row);
            if (name.IsNullOrEmpty()) {
                Log.Warning("{Kind} #{Id} is not currently mapped", kind, row.RowId);
            } else {
                Log.Warning("{Kind} #{Id} ({Name}) is not currently mapped", kind, row.RowId, name);
            }
        }

        foreach (var id in ids) {
            if (!excel.HasRow(id)) {
                Log.Warning("Layout contains {Kind} #{Id} which doesn't seem to exist", kind, id);
                continue;
            }

            if (!isValid(excel[id])) {
                Log.Warning("Layout contains {Kind} #{Id} which is invalid", kind, id);
            }
        }
    }

    private static void CheckMissingCollectionEntries<T>(string kind, List<LayoutCategory> categories, ExcelSheet<T> excel, Func<T, string> getName)
        where T : struct, IExcelRow<T> {
        CheckMissingCollectionEntries(kind, categories, excel, row => !getName(row).IsNullOrEmpty(), getName);
    }

    public void CheckMissingCollectionEntries() {
        CheckMissingCollectionEntries("Mount", Mounts, Plugin.DataManager.GetExcelSheet<Mount>(), it => it.Singular.ToString());
        CheckMissingCollectionEntries("Minion", Minions, Plugin.DataManager.GetExcelSheet<Companion>(), it => it.Singular.ToString());
        CheckMissingCollectionEntries("Barding", Bardings, Plugin.DataManager.GetExcelSheet<BuddyEquip>(), it => it.Name.ToString());
        CheckMissingCollectionEntries("Hairstyle", Hairstyles, Plugin.DataManager.GetExcelSheet<CharaMakeCustomize>(), it => it.IsPurchasable, _ => "");
        CheckMissingCollectionEntries("Facewear", Facewears, Plugin.DataManager.GetExcelSheet<Glasses>(), it => it.Name.ToString());
        CheckMissingCollectionEntries("Emote", Emotes, Plugin.DataManager.GetExcelSheet<Emote>(), it => it.Name.ToString());
        CheckMissingCollectionEntries("TT Card", TripleTriadCards, Plugin.DataManager.GetExcelSheet<TripleTriadCard>(), it => it.Name.ToString());
        // CheckMissingCollectionEntries("TT NPC", Minions, Plugin.DataManager.GetExcelSheet<Companion>(), it => it.Singular.ToString()); // TODO: there isn't even an IsUnlocked for it yet.
        CheckMissingCollectionEntries("Fashion Accessory", FashionAccessories, Plugin.DataManager.GetExcelSheet<Ornament>(), it => it.Singular.ToString());
        CheckMissingCollectionEntries("Framer's Kit", FramersKits, Plugin.DataManager.GetExcelSheet<GroupPoseFrame>(), it => it.Text.ToString());
        CheckMissingAchievements(Plugin.DataManager.GetExcelSheet<Achievement>());
    }

    public void CheckMissingAchievements(ExcelSheet<Achievement> excel) {
        var achievements = Achievements.SelectMany(it => it.GetAllAchievementIds()).ToList();

        foreach (var ach in excel) {
            if (IsAchievementInvalid(ach)) continue;

            if (!achievements.Contains(ach.RowId)) {
                Log.Warning("Achievement #{Id}, \"{Name}: {Desc}\", in {Category}/{Subcategory}, is not currently mapped",
                            ach.RowId, ach.Name.ToString(), ach.Description.ToString(),
                            ach.AchievementCategory.Value.AchievementKind.Value.Name.ToString(),
                            ach.AchievementCategory.Value.Name.ToString());
            }
        }

        foreach (var id in achievements) {
            if (!excel.HasRow(id)) {
                Log.Warning("Layout contains achievement #{Id} which doesn't seem to exist", id);
            }

            var ach = excel[id];

            if (IsAchievementInvalid(ach)) {
                Log.Warning("Layout contains achievement #{Id} which is invalid", id);
            }
        }
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutGroup), "group")]
[JsonDerivedType(typeof(AchievementLayoutCategory), "category")]
public abstract record AchievementLayout {
    public required string Name { get; init; }

    public abstract List<uint> GetAllAchievementIds();
    public abstract AchievementLayoutCategory? FindFirstCategory();
}

public record AchievementLayoutGroup : AchievementLayout {
    public required List<AchievementLayout> Items { get; init; }

    public string? Color { get; init; }
    public string? Icon { get; init; }

    public override List<uint> GetAllAchievementIds() => Items.SelectMany(it => it.GetAllAchievementIds()).ToList();
    public override AchievementLayoutCategory? FindFirstCategory() => Items.Select(it => it.FindFirstCategory()).FirstOrDefault(it => it != null);
}

public record AchievementLayoutCategory : AchievementLayout {
    public required List<AchievementLayoutItem> Items { get; init; }
    public required int Id { get; init; }
    public List<string> AdditionalViews { get; init; } = new();

    public override List<uint> GetAllAchievementIds() {
        return Items.SelectMany(it => it switch {
            AchievementLayoutItemSimple simple => [simple.Id],
            AchievementLayoutItemTiered tiered => tiered.Ids,
            _ => throw new ArgumentOutOfRangeException(nameof(it), it, null)
        }).ToList();
    }

    public override AchievementLayoutCategory FindFirstCategory() => this;
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutItemSimple), "simple")]
[JsonDerivedType(typeof(AchievementLayoutItemTiered), "tiered")]
public abstract record AchievementLayoutItem { }

public record AchievementLayoutItemSimple : AchievementLayoutItem {
    public required uint Id { get; init; }
}

public record AchievementLayoutItemTiered : AchievementLayoutItem {
    public required List<uint> Ids { get; init; }
    public bool Spoilers { get; init; } = false;
}

public record LayoutCategory {
    public required uint Id { get; init; }
    public required string Name { get; init; }
    public required List<uint> Items { get; init; }
}
