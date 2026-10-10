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

    public required List<AchievementLayoutGroup> Achievements { get; init; }

    public static MainLayout FromJson(JsonLayout json) {
        var nextCategoryId = 0;
        return new MainLayout {
            Achievements = json.AchievementCategories.Select(group => new AchievementLayoutGroup {
                Name = group.Name,
                Icon = group.Icon,
                Items = group.Categories.Select(category => new AchievementLayoutCategory {
                    Name = category.Name,
                    Id = nextCategoryId++,
                    Seasonal = category.Seasonal ?? false,
                    Items = category.Items.SelectMany(it => BuildItems(category.Name, it)).ToList(),
                }).ToList(),
            }).ToList()
        };
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
}

public class AchievementLayoutGroup : AchievementLayout {
    public required List<AchievementLayoutCategory> Items { get; init; }

    public string? Icon { get; init; }

    public override List<uint> GetAllAchievementIds() => Items.SelectMany(it => it.GetAllAchievementIds()).ToList();

    internal Dictionary<uint, uint> GetHighestIdMap() {
        var map = new Dictionary<uint, uint>();

        foreach (var category in Items) {
            foreach (var (key, value) in category.GetHighestIdMap()) {
                map[key] = value;
            }
        }

        return map;
    }
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

    internal Dictionary<uint, uint> GetHighestIdMap() {
        var map = new Dictionary<uint, uint>();

        foreach (var item in Items) {
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

        return map;
    }
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
