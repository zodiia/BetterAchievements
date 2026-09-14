using System;
using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.External.Lalachievements;
using BetterAchievements.Helpers;
using Lumina.Excel.Sheets;
using Emote = BetterAchievements.External.Lalachievements.Emote;
using Mount = BetterAchievements.External.Lalachievements.Mount;
using Title = BetterAchievements.External.Lalachievements.Title;
using TripleTriadCard = BetterAchievements.External.Lalachievements.TripleTriadCard;

namespace BetterAchievements.Services;

public class CollectionsService(Plugin plugin) {
    private const string OtherCategoryName = "Other";
    private const string UnknownCategoryName = "Unknown";

    public static readonly List<UnlockableType> Collections = [
        UnlockableType.Mount,
        UnlockableType.Minion,
        UnlockableType.Title,
        UnlockableType.TripleTriadCard,
        UnlockableType.Barding,
        UnlockableType.FashionAccessory,
        UnlockableType.Hairstyle,
        UnlockableType.Facewear,
        UnlockableType.Emote,
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

    private static List<CollectionCategory> BuildCategories(UnlockableType type, GameAllResponse response) {
        return type switch {
            UnlockableType.Mount => BuildCategories(type, response, response.GetTable<Mount>(), it => it.SourceTypeId),
            UnlockableType.Minion => BuildCategories(type, response, response.GetTable<Minion>(), it => it.SourceTypeId),
            UnlockableType.Title => BuildTitleCategory(response),
            UnlockableType.TripleTriadCard => BuildCategories(type, response, response.GetTable<TripleTriadCard>(), it => it.SourceTypeId),
            UnlockableType.Barding => BuildCategories(type, response, response.GetTable<Barding>(), it => it.SourceTypeId),
            UnlockableType.FashionAccessory => BuildCategories(type, response, response.GetTable<Fashion>(), it => it.SourceTypeId),
            UnlockableType.Hairstyle => BuildCategories(type, response, response.GetTable<Hair>(), it => it.SourceTypeId),
            UnlockableType.Facewear => BuildCategories(type, response, response.GetTable<Spectacle>(), it => it.SourceTypeId),
            UnlockableType.Emote => BuildCategories(type, response, response.GetTable<Emote>(), it => it.SourceTypeId),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
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

    private static List<CollectionCategory> BuildTitleCategory(GameAllResponse response) {
        var achievementNamesByTitleId = TitleUnlockAchievementNames.Value;

        var items = response.GetTable<Title>()
                            .Where(it => IsIncluded(UnlockableType.Title, it))
                            .OrderBy(it => it.Id)
                            .Select(ITable (it) => it with {
                                HowTo = achievementNamesByTitleId.TryGetValue(it.Id, out var achievementName)
                                    ? $"Obtain the achievement \"{achievementName}\"."
                                    : "Could not find the required achievement."
                            })
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
