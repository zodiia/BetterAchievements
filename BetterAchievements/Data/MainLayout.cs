using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using BetterAchievements.Helpers;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data;

public record MainLayout {
    public static readonly IPluginLog Log = Plugin.GetLogger<MainLayout>();

    public required List<AchievementLayout> Achievements { get; init; }

    public void CheckMissingAchievements(ExcelSheet<Achievement> excel) {
        var achievements = Achievements.SelectMany(it => it.GetAllAchievementIds()).ToList();

        foreach (var ach in excel) {
            if (!ach.IsValidEntry()) continue;

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

            if (!ach.IsValidEntry()) {
                Log.Warning("Layout contains achievement #{Id} which is invalid", id);
            }
        }
    }

    internal Dictionary<uint, uint> GetHighestIdMap() {
        var map = new Dictionary<uint, uint>();

        foreach (var layout in Achievements) {
            foreach (var (key, value) in layout.GetHighestIdMap()) {
                map[key] = value;
            }
        }

        return map;
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutGroup), "group")]
[JsonDerivedType(typeof(AchievementLayoutCategory), "category")]
public abstract record AchievementLayout {
    public required string Name { get; init; }

    public abstract List<uint> GetAllAchievementIds();
    public abstract AchievementLayoutCategory? FindFirstCategory();

    internal Dictionary<uint, uint> GetHighestIdMap() {
        var map = new Dictionary<uint, uint>();

        switch (this) {
            case AchievementLayoutGroup group:
                foreach (var subLayout in group.Items) {
                    foreach (var (key, value) in subLayout.GetHighestIdMap()) {
                        map[key] = value;
                    }
                }

                break;

            case AchievementLayoutCategory category:
                foreach (var item in category.Items) {
                    switch (item) {
                        case AchievementLayoutItemSimple simple:
                            map[simple.Id] = simple.Id;
                            break;
                        case AchievementLayoutItemTiered tiered:
                            var lastId = tiered.Ids.Last();
                            foreach (var id in tiered.Ids) {
                                map[id] = lastId;
                            }

                            break;
                    }
                }

                break;
        }

        return map;
    }
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
