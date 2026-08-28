using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.Helpers;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.Windows.Views;
using Lumina.Excel.Sheets;

namespace BetterAchievements.UI.Windows;

public class MainWindowState(Plugin plugin) {
    private const int FishingCategoryIdShift = 10000;
    private const int SpecialCategoryIdShift = 20000;
    public const int PinnedAchievementsCategoryId = SpecialCategoryIdShift + 0;
    public const int NoCategoryId = int.MinValue;

    private readonly MainLayout mainLayout = plugin.MainLayout;
    public Configuration Configuration = plugin.Configuration;

    private string currentSearch = "";
    private ulong achievementArrayHash = 0ul;

    private readonly Dictionary<int, VariableHeightClipper> achievementClippers = new();

    public MainLayout FilteredLayout { get; private set; } = plugin.MainLayout;
    public int SelectedCategoryId = NoCategoryId;
    public IView CurrentView { get; private set; } = new OverviewView(plugin.Configuration);
    public string SearchBuffer = "";
    public int AchievementPoints = 0;

    public Stopwatch DebugStopwatch = new();

    private const int FrameTimeHistorySize = 100;
    private readonly Queue<double> frameTimesMs = new(FrameTimeHistorySize);
    public double AverageFrameTimeMs { get; private set; }
    public double WorstFrameTimeMs { get; private set; }

    private bool MatchSearch(string name, string desc) {
        return name.Contains(currentSearch) || desc.Contains(currentSearch);
    }

    private bool MatchUnlockFilter(bool unlocked) {
        switch (Configuration.UnlockStatusFilter) {
            case UnlockStatusFilter.All: return true;
            case UnlockStatusFilter.Unlocked: return unlocked;
            case UnlockStatusFilter.Locked: return !unlocked;
        }

        throw new ArgumentOutOfRangeException($"{Configuration.UnlockStatusFilter} not implemented.");
    }

    private bool MatchRankedFilter(bool lalachievements) {
        switch (Configuration.RankedFilter) {
            case RankedFilter.All: return true;
            case RankedFilter.Lalachievements: return lalachievements;
        }

        throw new ArgumentOutOfRangeException($"{Configuration.RankedFilter} not implemented.");
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemSimple item) {
        var achievement = plugin.UnlockablesService.GetUnlockableAchievement(item.Id);
        return MatchSearch(achievement.NameLowercase(), achievement.DescriptionLowercase())
               && MatchUnlockFilter(achievement.Unlocked())
               && MatchRankedFilter(plugin.LalachievementsService.AchievementRarity.ContainsKey(achievement.Id()));
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemTiered item) {
        var achievements = plugin.UnlockablesService.GetUnlockableTieredAchievement(item.Ids, item.Spoilers);
        return MatchSearch(achievements.NameLowercase(), achievements.DescriptionLowercase())
               && MatchUnlockFilter(achievements.Unlocked())
               && MatchRankedFilter(plugin.LalachievementsService.AchievementRarity.ContainsKey(achievements.ProvidesAchievements().Last().Id()));
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemCombined item) {
        var achievements = plugin.UnlockablesService.GetUnlockableAchievement(item.Ids.Last());
        return MatchSearch(achievements.NameLowercase(), achievements.DescriptionLowercase())
               && MatchRankedFilter(plugin.LalachievementsService.AchievementRarity.ContainsKey(achievements.Id()));
        // && MatchUnlockFilter(achievements.Unlocked.All());
        // TODO
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItem item) {
        if (item is AchievementLayoutItemSimple simple) return FilterAchievementLayoutItem(simple);
        if (item is AchievementLayoutItemCombined combined) return FilterAchievementLayoutItem(combined);
        if (item is AchievementLayoutItemTiered tiered) return FilterAchievementLayoutItem(tiered);
        return false;
    }

    private AchievementLayoutCategory? FilterAchievementLayout(AchievementLayoutCategory category) {
        var items = category.Items.Where(FilterAchievementLayoutItem).ToList();
        if (items.Count == 0) return null;
        return category with { Items = items };
    }

    private AchievementLayoutGroup? FilterAchievementLayout(AchievementLayoutGroup group) {
        var items = group.Items.SelectMany<AchievementLayout, AchievementLayout>(it => {
            var res = FilterAchievementLayout(it);
            if (res == null) return [];
            return [res];
        }).ToList();
        if (items.Count == 0) return null;
        return group with { Items = items };
    }

    private AchievementLayout? FilterAchievementLayout(AchievementLayout layout) {
        if (layout is AchievementLayoutGroup group) return FilterAchievementLayout(group);
        if (layout is AchievementLayoutCategory category) return FilterAchievementLayout(category);
        return null;
    }

    private List<IUnlockable> SortedUnlockables(AchievementLayoutCategory category) {
        List<AchievementLayoutItem> sortedItems;

        if (Configuration.SortBy is SortBy.MostCommon or SortBy.Rarest) {
            sortedItems = category.Items.OrderBy(it => {
                if (it is AchievementLayoutItemSimple simple)
                    return plugin.LalachievementsService.AchievementRarity.GetValueOrDefault(simple.Id, uint.MaxValue);
                if (it is AchievementLayoutItemTiered tiered)
                    return plugin.LalachievementsService.AchievementRarity.GetValueOrDefault(tiered.Ids.Last(), uint.MaxValue);
                if (it is AchievementLayoutItemCombined combined) // TODO: fix
                    return plugin.LalachievementsService.AchievementRarity.GetValueOrDefault(combined.Ids.Last(), uint.MaxValue);
                return uint.MaxValue;
            }).ToList();
            if (Configuration.SortBy == SortBy.MostCommon) {
                sortedItems.Reverse();
            }
        } else {
            sortedItems = category.Items;
        }

        var unlockables = sortedItems.SelectMany<AchievementLayoutItem, IUnlockable>(it => {
            if (it is AchievementLayoutItemSimple simple)
                return [plugin.UnlockablesService.GetUnlockableAchievement(simple.Id)];
            if (it is AchievementLayoutItemTiered tiered)
                return [plugin.UnlockablesService.GetUnlockableTieredAchievement(tiered.Ids, tiered.Spoilers)];
            if (it is AchievementLayoutItemCombined combined) // TODO: fix
                return combined.Ids.Select(id => plugin.UnlockablesService.GetUnlockableAchievement(id)).ToList();
            return [];
        }).ToList();

        if (Configuration.SortBy == SortBy.Alphabetically) {
            unlockables.Sort((a, b) => string.Compare(a.NameLowercase(), b.NameLowercase(), StringComparison.OrdinalIgnoreCase));
        }

        return unlockables;
    }

    private void CalculateAchievementPoints() {
        var points = 0;
        foreach (var achievement in Plugin.DataManager.GetExcelSheet<Achievement>()) {
            if (Plugin.UnlockState.IsAchievementComplete(achievement)) {
                points += achievement.Points;
            }
        }

        AchievementPoints = points;
    }

    private void FilterAll() {
        var items = mainLayout.AchievementLayout.SelectMany<AchievementLayout, AchievementLayout>(it => {
            var res = FilterAchievementLayout(it);
            if (res == null) return [];
            return [res];
        }).ToList();
        FilteredLayout = new MainLayout { AchievementLayout = items };

        if (SelectedCategoryId == PinnedAchievementsCategoryId) {
            OpenPinnedAchievements();
        } else {
            SetCategory(SelectedCategoryId);
        }

        CalculateAchievementPoints();
    }

    private AchievementLayoutCategory? FindCategory(IEnumerable<AchievementLayout> group, int id) {
        foreach (var item in group) {
            switch (item) {
                case AchievementLayoutGroup subgroup:
                    var res = FindCategory(subgroup.Items, id);
                    if (res != null) {
                        return res;
                    }

                    break;
                case AchievementLayoutCategory category when category.Id == id:
                    return category;
            }
        }

        return null;
    }

    public void OpenPinnedAchievements() {
        SelectedCategoryId = PinnedAchievementsCategoryId;
        var unlockables = plugin.Configuration.PinnedAchievements
            .Select(id => plugin.UnlockablesService.GetExistingAchievement(id))
            .Where(it => it != null)
            .Select(it => it!)
            .ToList();
        CurrentView = new AchievementsView(PinnedAchievementsCategoryId, unlockables, Configuration, ClipperFor(PinnedAchievementsCategoryId));
    }

    public void Refresh() {
        plugin.UnlockablesService.Refresh();
        FilterAll();
        CalculateAchievementPoints();
    }

    public unsafe void CheckForUiRefresh() {
        var newAchievementArrayHash = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement.Instance()->CompletedAchievementsBitArray.ComputeHash();
        if (newAchievementArrayHash.Equals(achievementArrayHash) && !plugin.UnlockablesProgressService.CheckUpdated()) {
            return;
        }

        Refresh();
        achievementArrayHash = newAchievementArrayHash;
    }

    public void SetCategory(int categoryId) {
        var category = FindCategory(FilteredLayout.AchievementLayout, categoryId);
        if (category == null) {
            SelectedCategoryId = NoCategoryId;
            CurrentView = new OverviewView(Configuration);
            return;
        }

        SelectedCategoryId = categoryId;
        CurrentView = new AchievementsView(categoryId, SortedUnlockables(category), Configuration, ClipperFor(categoryId));
    }

    public void SetSearch(string search) {
        currentSearch = search.ToLower();
        FilterAll();
    }

    public void SetUnlockStatusFilter(UnlockStatusFilter unlockStatusFilter) {
        Configuration.UnlockStatusFilter = unlockStatusFilter;
        Configuration.Save();
        FilterAll();
    }

    public void SetContainsRewardsFilter(ContainsRewardsFilter containsRewardsFilter) {
        Configuration.ContainsRewardsFilter = containsRewardsFilter;
        Configuration.Save();
        FilterAll();
    }

    public void SetRankedFilter(RankedFilter rankedFilter) {
        Configuration.RankedFilter = rankedFilter;
        Configuration.Save();
        FilterAll();
    }

    public void SetAreaFilter(AreaFilter areaFilter) {
        Configuration.AreaFilter = areaFilter;
        Configuration.Save();
        FilterAll();
    }

    public void SetSortBy(SortBy sortBy) {
        Configuration.SortBy = sortBy;
        Configuration.Save();
        if (SelectedCategoryId != NoCategoryId && SelectedCategoryId != PinnedAchievementsCategoryId) {
            SetCategory(SelectedCategoryId);
        }
    }

    public void SetGroupBy(GroupBy groupBy) {
        Configuration.GroupBy = groupBy;
        Configuration.Save();
    }

    public void SetDisplayIds(bool displayIds) {
        Configuration.DisplayIds = displayIds;
        Configuration.Save();
    }

    public void SetNeverHideProgressBars(bool neverHideProgressBars) {
        Configuration.NeverHideProgressBars = neverHideProgressBars;
        Configuration.Save();
    }

    public void DebugStart() {
        if (!Configuration.DebugMode) return;
        DebugStopwatch.Restart();
    }

    public void DebugEnd() {
        if (!Configuration.DebugMode) return;
        DebugStopwatch.Stop();

        if (frameTimesMs.Count == FrameTimeHistorySize) frameTimesMs.Dequeue();
        frameTimesMs.Enqueue(DebugStopwatch.Elapsed.TotalMilliseconds);

        AverageFrameTimeMs = frameTimesMs.Average();
        WorstFrameTimeMs = frameTimesMs.Max();
    }

    private VariableHeightClipper ClipperFor(int categoryId) {
        if (!achievementClippers.TryGetValue(categoryId, out var clipper)) {
            clipper = new VariableHeightClipper();
            achievementClippers[categoryId] = clipper;
        }

        return clipper;
    }
}
