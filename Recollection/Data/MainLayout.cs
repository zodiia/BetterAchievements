using System;
using System.Collections.Generic;
using System.Linq;
using Recollection.Data.Layout;
using Recollection.Helpers;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace Recollection.Data;

public class MainLayout {
    public static readonly IPluginLog Log = Plugin.GetLogger<MainLayout>();

    public required List<AchievementLayout> Achievements { get; init; }

    public static MainLayout FromJson(JsonLayout json) {
        var nextCategoryId = 0;
        return new MainLayout { Achievements = json.AchievementCategories.Select(it => BuildLayout(it, ref nextCategoryId)).ToList() };
    }

    private static AchievementLayout BuildLayout(JsonAchievementCategory json, ref int nextCategoryId) {
        if (json is { Categories: not null, Items: not null }) {
            throw new InvalidOperationException($"Category \"{json.Name}\" cannot contain both categories and items");
        }

        if (json.Categories != null) {
            var children = new List<AchievementLayout>();
            foreach (var child in json.Categories) {
                children.Add(BuildLayout(child, ref nextCategoryId));
            }

            return new AchievementLayoutGroup { Name = json.Name, Icon = json.Icon, Items = children };
        }

        if (json.Items != null) {
            return new AchievementLayoutCategory {
                Name = json.Name,
                Id = nextCategoryId++,
                Seasonal = json.Seasonal ?? false,
                Items = json.Items.SelectMany(it => BuildItems(json.Name, it)).ToList(),
            };
        }

        throw new InvalidOperationException($"Category \"{json.Name}\" must contain either categories or items");
    }

    private static IEnumerable<AchievementLayoutItem> BuildItems(string categoryName, JsonAchievementItem json) {
        var spoilers = json.Spoilers ?? false;

        if (json is { Tiered: not null, Unique: not null }) {
            throw new InvalidOperationException($"An item in category \"{categoryName}\" cannot be both tiered and unique");
        }

        if (json.Tiered is { Count: > 0 }) {
            return [new AchievementLayoutItemTiered { Ids = json.Tiered, Spoilers = spoilers, Area = json.Area, HuntingLog = json.HuntingLog }];
        }

        if (json.Unique is { Count: > 0 }) {
            return json.Unique.Select(id => new AchievementLayoutItemSimple { Id = id, Spoilers = spoilers, Area = json.Area, HuntingLog = json.HuntingLog });
        }

        throw new InvalidOperationException($"An item in category \"{categoryName}\" must have a non-empty tiered or unique list");
    }

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

public abstract class AchievementLayout {
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

public class AchievementLayoutGroup : AchievementLayout {
    public required List<AchievementLayout> Items { get; init; }

    public string? Icon { get; init; }

    public override List<uint> GetAllAchievementIds() => Items.SelectMany(it => it.GetAllAchievementIds()).ToList();
    public override AchievementLayoutCategory? FindFirstCategory() => Items.Select(it => it.FindFirstCategory()).FirstOrDefault(it => it != null);
}

public class AchievementLayoutCategory : AchievementLayout {
    public required List<AchievementLayoutItem> Items { get; init; }
    public required int Id { get; init; }
    public bool Seasonal { get; init; } = false;

    public override List<uint> GetAllAchievementIds() {
        return Items.SelectMany(it => it switch {
            AchievementLayoutItemSimple simple => [simple.Id],
            AchievementLayoutItemTiered tiered => tiered.Ids,
            _ => throw new ArgumentOutOfRangeException(nameof(it), it, null)
        }).ToList();
    }

    public override AchievementLayoutCategory FindFirstCategory() => this;
}

public abstract class AchievementLayoutItem {
    public bool Spoilers { get; init; } = false;
    public uint? Area { get; init; }
    public uint? HuntingLog { get; init; }
}

public class AchievementLayoutItemSimple : AchievementLayoutItem {
    public required uint Id { get; init; }
}

public class AchievementLayoutItemTiered : AchievementLayoutItem {
    public required List<uint> Ids { get; init; }
}
