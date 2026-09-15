using System;
using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.External.Lalachievements;
using BetterAchievements.Helpers;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Emote = BetterAchievements.External.Lalachievements.Emote;
using Mount = BetterAchievements.External.Lalachievements.Mount;
using Title = BetterAchievements.External.Lalachievements.Title;
using TripleTriadCard = BetterAchievements.External.Lalachievements.TripleTriadCard;

namespace BetterAchievements.Services;

public class CollectionsService(Plugin plugin) {
    private const string OtherCategoryName = "Other";
    private const string UnknownCategoryName = "Unknown";
    private const uint MaxRecordableGatheringItemId = 10000;

    public static readonly List<UnlockableType> Collections = [
        UnlockableType.Mount,
        UnlockableType.Minion,
        UnlockableType.Title,
        UnlockableType.TripleTriadCard,
        UnlockableType.TripleTriadNpc,
        UnlockableType.Barding,
        UnlockableType.FashionAccessory,
        UnlockableType.Hairstyle,
        UnlockableType.Facewear,
        UnlockableType.Emote,
        UnlockableType.CraftingLog,
        UnlockableType.GatheringLog,
    ];

    private readonly Dictionary<UnlockableType, List<CollectionCategory>> categories = new();
    public bool Loaded { get; private set; } = false;

    public static string Label(UnlockableType type) => type switch {
        UnlockableType.Achievement => "Achievements",
        UnlockableType.Mount => "Mounts",
        UnlockableType.Minion => "Minions",
        UnlockableType.Title => "Titles",
        UnlockableType.TripleTriadCard => "Triple Triad Cards",
        UnlockableType.TripleTriadNpc => "Triple Triad NPCs",
        UnlockableType.Barding => "Bardings",
        UnlockableType.FashionAccessory => "Fashion Accessories",
        UnlockableType.Hairstyle => "Hairstyles",
        UnlockableType.Facewear => "Facewears",
        UnlockableType.Emote => "Emotes",
        UnlockableType.CraftingLog => "Crafting Log",
        UnlockableType.Fish => "Fishing",
        UnlockableType.Spearfish => "Spearfishing",
        UnlockableType.FramersKit => "Framer's Kits",
        UnlockableType.HuntingLog => "Hunting Log",
        UnlockableType.GatheringLog => "Gathering Log",
        UnlockableType.OrchestrionRoll => "Orchestrion Rolls",
        UnlockableType.AetherCurrent => "Aether Currents",
        UnlockableType.FieldRecord => "Field Records",
        UnlockableType.OccultRecord => "Occult Records",
        UnlockableType.SurveyRecord => "Survey Records",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public List<CollectionCategory> Categories(UnlockableType type) {
        return categories.GetValueOrDefault(type, []);
    }

    public CollectionCategory? FindCategory(UnlockableType type, uint id) {
        return Categories(type).FirstOrDefault(it => it.Id == id);
    }

    public bool CheckForUpdates() {
        if (plugin.LalachievementsService.GameAllLoaded == Loaded || plugin.LalachievementsService.GameAll == null) {
            return false;
        }

        Rebuild(plugin.LalachievementsService.GameAll);
        Loaded = plugin.LalachievementsService.GameAllLoaded;
        return true;
    }

    private void Rebuild(GameAllResponse response) {
        categories.Clear();

        foreach (var type in Collections) {
            categories[type] = BuildCategories(type, response);
        }
    }

    private static List<CollectionCategory> BuildCategories(UnlockableType type, GameAllResponse response) => type switch {
        UnlockableType.Mount => BuildCategoriesWithGameAll(response, response.GetTable<Mount>(), it => it.SourceTypeId),
        UnlockableType.Minion => BuildCategoriesWithGameAll(response, response.GetTable<Minion>(), it => it.SourceTypeId),
        UnlockableType.TripleTriadCard => BuildCategoriesWithGameAll(response, response.GetTable<TripleTriadCard>(), it => it.SourceTypeId),
        UnlockableType.Barding => BuildCategoriesWithGameAll(response, response.GetTable<Barding>(), it => it.SourceTypeId),
        UnlockableType.FashionAccessory => BuildCategoriesWithGameAll(response, response.GetTable<Fashion>(), it => it.SourceTypeId),
        UnlockableType.Hairstyle => BuildCategoriesWithGameAll(response, response.GetTable<Hair>(), it => it.SourceTypeId),
        UnlockableType.Facewear => BuildCategoriesWithGameAll(response, response.GetTable<Spectacle>(), it => it.SourceTypeId),
        UnlockableType.Emote => BuildCategoriesWithGameAll(response, response.GetTable<Emote>(), it => it.SourceTypeId),
        UnlockableType.Title => BuildSingleCategoryWithGameAll(response.GetTable<Title>(), UnlockableType.Title),
        UnlockableType.TripleTriadNpc => BuildSingleCategoryWithGameAll(response.GetTable<TripleTriadNpc>(), UnlockableType.TripleTriadNpc),
        UnlockableType.CraftingLog => BuildCraftingCategories(),
        UnlockableType.GatheringLog => BuildGatheringCategories(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private static List<CollectionCategory> BuildCategoriesWithGameAll<T>(GameAllResponse response, List<T> rows, Func<T, uint?> getSourceTypeId)
        where T : ITableRow => rows.Where(it => it.Deleted is false)
                                   .GroupBy(it => response.GetSourceType(getSourceTypeId(it)))
                                   .Select(group => new CollectionCategory {
                                       Id = group.Key.Id,
                                       Name = group.Key.Name,
                                       Items = group.OrderBy(it => it.Id).Select(it => it.Id).ToList()
                                   })
                                   .Where(it => it.Items.Count > 0)
                                   .OrderBy(it => CategorySortRank(it.Name))
                                   .ThenBy(it => it.Id)
                                   .ToList();

    private static List<CollectionCategory> BuildSingleCategoryWithGameAll<T>(List<T> rows, UnlockableType type) where T : ITableRow => [
        new() { Id = 0, Name = Label(type), Items = rows.Where(it => it.Deleted == false).OrderBy(it => it.Id).Select(it => it.Id).ToList() }
    ];

    private static List<CollectionCategory> BuildCraftingCategories() =>
        ExcelSheets.Recipe.Value.Where(it => it.IsValidEntry())
                   .GroupBy(it => it.CraftType.RowId)
                   .Select(group => new CollectionCategory {
                       Id = group.Key,
                       Name = group.First().CraftType.Value.Name.ToString(),
                       Items = group.Where(it => it.IsValidEntry()).Select(it => it.RowId)
                                    .ToList(),
                   })
                   .ToList();

    private static List<CollectionCategory> BuildGatheringCategories() =>
        ExcelSheets.GatheringPoint.Value
                   .Where(it => it.IsValidEntry())
                   .GroupBy(it => it.GatheringPointBase.Value.GatheringType, new GatheringTypeComparer())
                   .Select(group => new CollectionCategory {
                       Id = group.Key.RowId,
                       Name = group.Key.Value.Name.ToString(),
                       Items = group.SelectMany(it => it.GatheringPointBase.Value.Item
                                                        .Where(row => row.GetValueOrDefault<GatheringItem>()?.IsValidEntry() ?? false)
                                                        .Select(row => row.RowId))
                                    .GroupBy(row => row)
                                    .Select(rows => rows.First())
                                    .Order()
                                    .ToList(),
                   })
                   .OrderBy(it => it.Id)
                   .ToList();

    /// <summary>Orders categories by not changing their order, except putting Other and Unknown as the last two</summary>
    /// <param name="name">Category name</param>
    /// <returns>1 for Other, 2 for Unknown, 0 otherwise</returns>
    private static int CategorySortRank(string name) => name switch {
        OtherCategoryName => 1,
        UnknownCategoryName => 2,
        _ => 0
    };
}

// c# what do you make me do...
class GatheringTypeComparer : IEqualityComparer<RowRef<GatheringType>> {
    public bool Equals(RowRef<GatheringType> x, RowRef<GatheringType> y) => x.RowId == y.RowId;
    public int GetHashCode(RowRef<GatheringType> obj) => base.GetHashCode();
}
