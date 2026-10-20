using MethodTimer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Plugin.Services;
using Recollection.Data;
using Recollection.Data.Unlockable;
using Recollection.External.Lalachievements;
using Recollection.Helpers;
using Recollection.Services;

namespace Recollection.UI.State;

public class UnlockablesState(Plugin plugin) {
    private const int ActivityRowCount = 10;

    private readonly IPluginLog log = Plugin.GetLogger<UnlockablesState>();
    private readonly Configuration configuration = plugin.Configuration;
    private readonly MainLayout mainLayout = plugin.MainLayout;
    private readonly UnlockablesService unlockablesService = plugin.UnlockablesService;
    private readonly CollectionsService collectionsService = plugin.CollectionsService;
    private readonly DatabaseService databaseService = plugin.DatabaseService;
    private readonly AchievementProgressService achievementProgressService = plugin.AchievementProgressService;
    private readonly LalachievementsService lalachievementsService = plugin.LalachievementsService;
    private readonly Dictionary<AchievementLayout, List<uint>> progressAchievementIds = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<AchievementLayout, AchievementProgress> progressCache = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CollectionCategory, CollectionProgress> collectionCategoryCache = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<UnlockableType, CollectionProgress> collectionCache = new();

    private List<uint> pinnedSnapshot = [];
    private AchievementProgress? pinnedProgressCache;
    private AchievementProgress? overallProgressCache;

    private string search = "";

    public MainLayout FilteredLayout { get; private set; } = plugin.MainLayout;
    public Score Achievement { get; private set; } = new(0, 0);
    public List<AchievementUpdate> RecentlyUnlockedAchievements { get; private set; } = [];
    public List<AchievementCompletionRatio> NearingCompletionAchievements { get; private set; } = [];

    public void SetSearch(string value) {
        search = value.ToLower();
        ApplyFilters();
    }

    [Time]
    public void ApplyFilters() {
        progressAchievementIds.Clear();

        FilteredLayout = new MainLayout { Achievements = mainLayout.Achievements.Select(FilterAchievementLayout).OfType<AchievementLayoutGroup>().ToList() };
        progressCache.Clear();
        collectionCategoryCache.Clear();
        collectionCache.Clear();
        pinnedProgressCache = null;
        overallProgressCache = null;
    }

    [Time]
    public void Refresh() {
        unlockablesService.Refresh();
        ApplyFilters();
        Achievement = UnlockablesService.CalculateAchievementPoints();
        RefreshActivity();
    }

    [Time]
    public void RefreshActivity() {
        RecentlyUnlockedAchievements = databaseService.GetLastUnlockedAchievements(Plugin.PlayerState.ContentId);
        NearingCompletionAchievements = mainLayout.Achievements
                                                  .SelectMany(group => group.Items)
                                                  .SelectMany(category => category.Sections)
                                                  .SelectMany(section => section.Items)
                                                  .Select(CompletionRatio)
                                                  .Where(it => it.Ratio < 1.0)
                                                  .OrderByDescending(it => it.Ratio)
                                                  .Take(ActivityRowCount)
                                                  .ToList();
    }

    public bool CollectionsLoaded => collectionsService.Loaded;

    public bool CheckForUpdates() {
        if (collectionsService.CheckForUpdates() ||
            unlockablesService.GetAchievementListUpdatedForUi() ||
            achievementProgressService.CheckUpdated()) {
            Refresh();
            return true;
        }

        unlockablesService.CheckPolledUnlocks();

        if (unlockablesService.HasChanges) {
            ApplyChanges(unlockablesService.GetUpdates());
        }

        return false;
    }

    [Time]
    private void ApplyChanges(List<UnlockableKey> changes) {
        var unlockedAchievementIds = changes.Where(it => it.Type == UnlockableType.Achievement).Select(it => it.Id).ToHashSet();
        var unlockedCollectionTypes = changes.Where(it => it.Type != UnlockableType.Achievement).Select(it => it.Type).ToHashSet();

        if (unlockedAchievementIds.Count > 0) {
            var staleLayouts = progressCache.Keys.Where(layout => progressAchievementIds.GetValueOrDefault(layout, layout.GetAllAchievementIds()).Any(unlockedAchievementIds.Contains)).ToList();
            staleLayouts.ForEach(layout => progressCache.Remove(layout));
            overallProgressCache = null;
            if (pinnedSnapshot.Any(unlockedAchievementIds.Contains)) pinnedProgressCache = null;
            RefreshActivity();
        }

        foreach (var type in unlockedCollectionTypes) {
            collectionCache.Remove(type);
            CollectionCategories(type).ForEach(category => collectionCategoryCache.Remove(category));
        }
    }

    public AchievementProgress GetProgress(AchievementLayout layout) {
        if (progressCache.TryGetValue(layout, out var cached)) return cached;

        var ids = progressAchievementIds.GetValueOrDefault(layout, layout.GetAllAchievementIds());
        var result = unlockablesService.CalculateAchievementProgress(ids);
        progressCache[layout] = result;
        return result;
    }

    public AchievementProgress GetOverallProgress() {
        return overallProgressCache ??= SumProgress(mainLayout.Achievements.Select(layout => GetProgress(layout)));
    }

    public AchievementProgress GetPinnedProgress() {
        var pinned = unlockablesService.GetPinned();
        if (pinnedProgressCache is { } cached && pinned.SequenceEqual(pinnedSnapshot)) return cached;

        pinnedSnapshot = pinned.ToList();
        return pinnedProgressCache = unlockablesService.CalculateAchievementProgress(pinnedSnapshot);
    }

    [Time]
    public CollectionProgress GetProgress(UnlockableType type, CollectionCategory category) {
        if (collectionCategoryCache.TryGetValue(category, out var cached)) return cached;

        uint obtained = 0;
        uint total = 0;
        uint visible = 0;

        foreach (var item in category.Items) {
            var unlockable = unlockablesService.GetUnlockable(type, item);
            if (!MatchSearch(unlockable)) continue;

            total++;
            if (unlockable.Unlocked) obtained++;
            if (MatchUnlockFilter(unlockable.Unlocked)) visible++;
        }

        var result = new CollectionProgress(new Score(obtained, total), visible);
        collectionCategoryCache[category] = result;
        return result;
    }

    public CollectionProgress GetProgress(UnlockableType type) {
        if (collectionCache.TryGetValue(type, out var cached)) return cached;

        uint obtained = 0;
        uint total = 0;
        uint visible = 0;

        foreach (var category in CollectionCategories(type)) {
            var (score, categoryVisible) = GetProgress(type, category);
            obtained += score.Obtained;
            total += score.Total;
            visible += categoryVisible;
        }

        var result = new CollectionProgress(new Score(obtained, total), visible);
        collectionCache[type] = result;
        return result;
    }

    private static AchievementProgress SumProgress(IEnumerable<AchievementProgress> items) {
        uint obtainedCount = 0, totalCount = 0, obtainedPoints = 0, totalPoints = 0;

        foreach (var (count, points) in items) {
            obtainedCount += count.Obtained;
            totalCount += count.Total;
            obtainedPoints += points.Obtained;
            totalPoints += points.Total;
        }

        return new AchievementProgress(new Score(obtainedCount, totalCount), new Score(obtainedPoints, totalPoints));
    }

    public CategoryWithBreadcrumbs? FindCategory(int id) {
        return FindCategory(FilteredLayout.Achievements, category => category.Id == id);
    }

    public string? FindBreadcrumb(uint achievementId) {
        return FindCategory(mainLayout.Achievements, category => category.GetAllAchievementIds().Contains(achievementId))?.Breadcrumb;
    }

    public AchievementLayoutGroup? FindTopLevelGroup(string name) {
        return FilteredLayout.Achievements.FirstOrDefault(it => it.Name == name);
    }

    public List<CollectionCategory> CollectionCategories(UnlockableType type) {
        return collectionsService.Categories(type);
    }

    public CollectionCategory? FindCollectionCategory(UnlockableType type, uint id) {
        return collectionsService.FindCategory(type, id);
    }

    public List<UnlockableSection> SortedUnlockables(AchievementLayoutCategory category) {
        var unfiltered = mainLayout.Achievements.SelectMany(group => group.Items).First(it => it.Id == category.Id).Sections;
        return category.Sections.Select(section => new UnlockableSection(section.Name, SortedUnlockables(section), unfiltered.First(it => it.Name == section.Name))).ToList();
    }

    private List<IUnlockable> SortedUnlockables(AchievementLayoutSection section) {
        var items = section.Items;

        if (configuration.SortBy is SortBy.MostCommon or SortBy.Rarest) {
            items = items.OrderBy(Rarity).ToList();
            if (configuration.SortBy == SortBy.MostCommon) items.Reverse();
        }

        var unlockables = items.SelectMany<AchievementLayoutItem, IUnlockable>(it => {
            return it switch {
                AchievementLayoutItemSimple simple => [
                    unlockablesService.GetUnlockableAchievement(simple.Id)
                ],
                AchievementLayoutItemTiered tiered => [
                    unlockablesService.GetUnlockableTieredAchievement(
                        tiered.Ids, tiered.Spoilers)
                ],
                _ => []
            };
        }).ToList();

        if (configuration.SortBy == SortBy.Alphabetically)
            unlockables.Sort((a, b) => string.Compare(a.NameLowercase, b.NameLowercase, StringComparison.OrdinalIgnoreCase));

        return unlockables;
    }

    [Time]
    public List<IUnlockable> SortedCollectionUnlockables(UnlockableType type, CollectionCategory category) {
        var unlockables = category.Items
                                  .Select(it => unlockablesService.GetUnlockable(type, it))
                                  .Where(it => MatchSearch(it) && MatchUnlockFilter(it.Unlocked))
                                  .ToList();

        if (configuration.SortBy == SortBy.Alphabetically)
            unlockables.Sort((a, b) => string.Compare(a.NameLowercase, b.NameLowercase, StringComparison.OrdinalIgnoreCase));

        return unlockables;
    }

    public List<IUnlockable> PinnedUnlockables() {
        return unlockablesService.GetPinnedUnlockables();
    }

    public List<IUnlockable> PinnedUnlockables(AchievementLayout layout) {
        var ids = layout.GetAllAchievementIds().ToHashSet();
        return PinnedUnlockables().Where(it => ids.Contains(it.Id)).ToList();
    }

    private static CategoryWithBreadcrumbs? FindCategory(IEnumerable<AchievementLayoutGroup> groups, Func<AchievementLayoutCategory, bool> predicate) {
        foreach (var group in groups)
            foreach (var category in group.Items)
                if (predicate(category))
                    return new CategoryWithBreadcrumbs(category, $"{group.Name} / {category.Name}");

        return null;
    }

    private AchievementCompletionRatio CompletionRatio(AchievementLayoutItem item) {
        return item switch {
            AchievementLayoutItemSimple simple => unlockablesService.GetUnlockableAchievement(simple.Id).AchievementCompletionRatio(),
            AchievementLayoutItemTiered tiered => unlockablesService.GetUnlockableTieredAchievement(tiered.Ids, tiered.Spoilers).AchievementCompletionRatio(),
            _ => throw new InvalidDataException("Unmapped achievement layout item type")
        };
    }

    private uint Rarity(AchievementLayoutItem item) {
        return item switch {
            AchievementLayoutItemSimple simple => lalachievementsService.AchievementRarity.GetValueOrDefault(simple.Id, uint.MaxValue),
            AchievementLayoutItemTiered tiered => lalachievementsService.AchievementRarity.GetValueOrDefault(tiered.Ids.Last(), uint.MaxValue),
            _ => uint.MaxValue
        };
    }

    private bool MatchSearch(IUnlockable unlockable) {
        return unlockable.NameLowercase.Contains(search)
               || unlockable.DescriptionLowercase?.Contains(search) == true
               || unlockable.HowToLowercase?.Contains(search) == true;
    }

    private bool MatchUnlockFilter(bool unlocked) {
        return configuration.UnlockStatusFilter switch {
            UnlockStatusFilter.All => true,
            UnlockStatusFilter.Unlocked => unlocked,
            UnlockStatusFilter.Locked => !unlocked,
            _ => throw new ArgumentOutOfRangeException($"{configuration.UnlockStatusFilter} not implemented.")
        };
    }

    private bool MatchRankedFilter(bool lalachievements) {
        return configuration.RankedFilter switch {
            RankedFilter.All => true,
            RankedFilter.Lalachievements => lalachievements,
            _ => throw new ArgumentOutOfRangeException($"{configuration.RankedFilter} not implemented.")
        };
    }

    private bool MatchAreaFilter(List<uint>? area) {
        if (configuration.AreaFilter == AreaFilter.All) return true;
        if (area == null || ExcelSheets.TerritoryType.Value.GetRowOrDefault(Plugin.ClientState.TerritoryType) is not { } current) return false;

        return configuration.AreaFilter switch {
            AreaFilter.Region => area.Any(id => ExcelSheets.TerritoryType.Value.GetRowOrDefault(id)?.PlaceNameRegion.RowId == current.PlaceNameRegion.RowId),
            AreaFilter.Zone => area.Any(id => ExcelSheets.TerritoryType.Value.GetRowOrDefault(id)?.PlaceName.RowId == current.PlaceName.RowId),
            _ => throw new ArgumentOutOfRangeException($"{configuration.AreaFilter} not implemented.")
        };
    }

    private bool MatchProgressionFilter(uint? requiredQuest) {
        return configuration.ProgressionFilter switch {
            ProgressionFilter.All => true,
            ProgressionFilter.OnlyAvailable => requiredQuest == null || Plugin.UnlockState.IsQuestCompleted(ExcelSheets.Quest.Value.GetRow(requiredQuest.Value)),
            _ => throw new ArgumentOutOfRangeException($"{configuration.ProgressionFilter} not implemented.")
        };
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemSimple item, bool seasonal) {
        var achievement = unlockablesService.GetUnlockableAchievement(item.Id);
        return MatchSearch(achievement)
               && MatchUnlockFilter(achievement.Unlocked)
               && (!seasonal || achievement.Unlocked)
               && MatchRankedFilter(lalachievementsService.AchievementRarity.ContainsKey(achievement.Id));
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemTiered item, bool seasonal) {
        var achievements = unlockablesService.GetUnlockableTieredAchievement(item.Ids, item.Spoilers);
        return MatchSearch(achievements)
               && MatchUnlockFilter(achievements.Unlocked)
               && (!seasonal || achievements.Unlocked)
               && MatchRankedFilter(lalachievementsService.AchievementRarity.ContainsKey(achievements.ProvidesAchievements.Last().Id));
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItem item, bool seasonal) {
        if (!MatchAreaFilter(item.Area) || !MatchProgressionFilter(item.RequiredQuest)) return false;

        return item switch {
            AchievementLayoutItemSimple simple => FilterAchievementLayoutItem(simple, seasonal),
            AchievementLayoutItemTiered tiered => FilterAchievementLayoutItem(tiered, seasonal),
            _ => false
        };
    }

    private bool MatchProgressFilter(AchievementLayoutItemSimple item, bool seasonal) {
        var achievement = unlockablesService.GetUnlockableAchievement(item.Id);
        return MatchSearch(achievement)
               && (!seasonal || achievement.Unlocked)
               && MatchRankedFilter(lalachievementsService.AchievementRarity.ContainsKey(achievement.Id));
    }

    private bool MatchProgressFilter(AchievementLayoutItemTiered item, bool seasonal) {
        var achievements = unlockablesService.GetUnlockableTieredAchievement(item.Ids, item.Spoilers);
        return MatchSearch(achievements)
               && (!seasonal || achievements.Unlocked)
               && MatchRankedFilter(lalachievementsService.AchievementRarity.ContainsKey(achievements.ProvidesAchievements.Last().Id));
    }

    private bool MatchProgressFilter(AchievementLayoutItem item, bool seasonal) {
        if (!MatchAreaFilter(item.Area) || !MatchProgressionFilter(item.RequiredQuest)) return false;

        return item switch {
            AchievementLayoutItemSimple simple => MatchProgressFilter(simple, seasonal),
            AchievementLayoutItemTiered tiered => MatchProgressFilter(tiered, seasonal),
            _ => false
        };
    }

    private static List<uint> AchievementIds(AchievementLayoutItem item) {
        return item switch {
            AchievementLayoutItemSimple simple => [simple.Id],
            AchievementLayoutItemTiered tiered => tiered.Ids,
            _ => []
        };
    }

    private AchievementLayoutSection? FilterAchievementLayout(AchievementLayoutSection section) {
        var items = section.Items.Where(it => FilterAchievementLayoutItem(it, section.Seasonal)).ToList();
        if (items.Count == 0) return null;

        return new AchievementLayoutSection { Name = section.Name, Seasonal = section.Seasonal, Items = items };
    }

    private AchievementLayoutCategory? FilterAchievementLayout(AchievementLayoutCategory category) {
        var progressIds = category.Sections.SelectMany(section => section.Items.Where(it => MatchProgressFilter(it, section.Seasonal))).SelectMany(AchievementIds).ToList();
        progressAchievementIds[category] = progressIds;

        var sections = category.Sections.Select(FilterAchievementLayout).OfType<AchievementLayoutSection>().ToList();
        if (sections.Count == 0) return null;

        var filtered = new AchievementLayoutCategory { Name = category.Name, Id = category.Id, Sections = sections };
        progressAchievementIds[filtered] = progressIds;
        return filtered;
    }

    private AchievementLayoutGroup? FilterAchievementLayout(AchievementLayoutGroup group) {
        var filteredChildren = group.Items.Select(FilterAchievementLayout).OfType<AchievementLayoutCategory>().ToList();

        var progressIds = group.Items.SelectMany(child => progressAchievementIds[child]).ToList();
        progressAchievementIds[group] = progressIds;

        if (filteredChildren.Count == 0) return null;

        var filtered = new AchievementLayoutGroup { Name = group.Name, Icon = group.Icon, Items = filteredChildren };
        progressAchievementIds[filtered] = progressIds;
        return filtered;
    }
}
