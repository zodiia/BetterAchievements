using System;
using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.External.Lalachievements;

namespace BetterAchievements.Services;

public class CollectionsService(Plugin plugin) {
    private const uint TitlesCategoryId = 0;
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
    private GameAllResponse? builtFrom;

    public bool Loaded => builtFrom != null;

    public static string Label(UnlockableType type) {
        return type switch {
            UnlockableType.Achievement => "Achievements",
            UnlockableType.Mount => "Mounts",
            UnlockableType.Minion => "Minions",
            UnlockableType.Title => "Titles",
            UnlockableType.TripleTriadCard => "Triple Triad Cards",
            UnlockableType.Barding => "Bardings",
            UnlockableType.FashionAccessory => "Fashion Accessories",
            UnlockableType.Hairstyle => "Hairstyles",
            UnlockableType.Facewear => "Facewears",
            UnlockableType.Emote => "Emotes",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public List<CollectionCategory> Categories(UnlockableType type) {
        return categories.GetValueOrDefault(type, []);
    }

    public CollectionCategory? FindCategory(UnlockableType type, uint id) {
        return Categories(type).FirstOrDefault(it => it.Id == id);
    }

    public bool CheckForUpdates() {
        var response = plugin.LalachievementsService.GameAll;
        if (response == null || ReferenceEquals(response, builtFrom)) return false;

        Rebuild(response);
        return true;
    }

    private void Rebuild(GameAllResponse response) {
        categories.Clear();

        foreach (var type in Collections) {
            categories[type] = BuildCategories(type, response);
        }

        builtFrom = response;
    }

    private static List<CollectionCategory> BuildCategories(UnlockableType type, GameAllResponse response) {
        return type switch {
            UnlockableType.Mount => BuildCategories(type, response, response.GetTable<Mount>(), it => it.HowTo, it => it.SourceTypeId),
            UnlockableType.Minion => BuildCategories(type, response, response.GetTable<Minion>(), it => it.HowTo, it => it.SourceTypeId),
            UnlockableType.Title => BuildTitleCategory(response),
            UnlockableType.TripleTriadCard =>
                BuildCategories(type, response, response.GetTable<TripleTriadCard>(), it => it.HowTo, it => it.SourceTypeId),
            UnlockableType.Barding => BuildCategories(type, response, response.GetTable<Barding>(), it => it.HowTo, it => it.SourceTypeId),
            UnlockableType.FashionAccessory => BuildCategories(type, response, response.GetTable<Fashion>(), it => it.HowTo, it => it.SourceTypeId),
            UnlockableType.Hairstyle => BuildCategories(type, response, response.GetTable<Hair>(), it => it.HowTo, it => it.SourceTypeId),
            UnlockableType.Facewear => BuildCategories(type, response, response.GetTable<Spectacle>(), it => it.HowTo, it => it.SourceTypeId),
            UnlockableType.Emote => BuildCategories(type, response, response.GetTable<Emote>(), it => it.HowTo, it => it.SourceTypeId),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static List<CollectionCategory> BuildCategories<T>(
        UnlockableType type, GameAllResponse response, List<T> rows, Func<T, string?> getHowTo, Func<T, uint?> getSourceTypeId)
        where T : ITable {
        return rows.Where(it => IsIncluded(type, it))
                   .GroupBy(it => response.GetSourceType(getSourceTypeId(it)))
                   .Select(group => new CollectionCategory {
                       Id = group.Key.Id,
                       Name = group.Key.Name,
                       Items = group.OrderBy(it => it.Id)
                                    .Select(it => new CollectionItem {
                                        Id = it.Id,
                                        Table = it,
                                        SourceType = group.Key,
                                        HowTo = getHowTo(it) ?? ""
                                    })
                                    .ToList()
                   })
                   .Where(it => it.Items.Count > 0)
                   .OrderBy(it => CategorySortRank(it.Name))
                   .ThenBy(it => it.Id)
                   .ToList();
    }

    private static int CategorySortRank(string name) {
        if (name == OtherCategoryName) return 1;
        if (name == UnknownCategoryName) return 2;
        return 0;
    }

    private static List<CollectionCategory> BuildTitleCategory(GameAllResponse response) {
        var items = response.GetTable<Title>()
                            .Where(it => IsIncluded(UnlockableType.Title, it))
                            .OrderBy(it => it.Id)
                            .Select(it => new CollectionItem { Id = it.Id, Table = it, SourceType = null, HowTo = "" })
                            .ToList();

        return [new CollectionCategory { Id = TitlesCategoryId, Name = Label(UnlockableType.Title), Items = items }];
    }

    private static bool IsIncluded(UnlockableType type, ITable row) {
        return row.Deleted != true && UnlockablesService.IsValidCollectionItem(type, row.Id);
    }
}
