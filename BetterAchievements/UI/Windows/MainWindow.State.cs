using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.Helpers;
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
    private AchievementLayoutCategory pinnedAchievementsCategory = new() { Items = [], Id = PinnedAchievementsCategoryId, Name = "Pinned" };
    public MainLayout FilteredLayout { get; private set; } = plugin.MainLayout;
    public int SelectedCategoryId = NoCategoryId;
    public AchievementLayoutCategory? SelectedAchievementCategory { get; private set; }
    public List<IUnlockable> CategoryUnlockables { get; private set; } = new();
    public string SearchBuffer = "";
    public int AchievementPoints = 0;

    public Stopwatch DebugStopwatch = new();

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

    private void SortCategory() {
        if (SelectedAchievementCategory == null) return;
        List<AchievementLayoutItem> sortedItems;

        if (Configuration.SortBy is SortBy.MostCommon or SortBy.Rarest) {
            sortedItems = SelectedAchievementCategory.Items.OrderBy(it => {
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
            sortedItems = SelectedAchievementCategory.Items;
        }

        CategoryUnlockables = sortedItems.SelectMany<AchievementLayoutItem, IUnlockable>(it => {
            if (it is AchievementLayoutItemSimple simple)
                return [plugin.UnlockablesService.GetUnlockableAchievement(simple.Id)];
            if (it is AchievementLayoutItemTiered tiered)
                return [plugin.UnlockablesService.GetUnlockableTieredAchievement(tiered.Ids, tiered.Spoilers)];
            if (it is AchievementLayoutItemCombined combined) // TODO: fix
                return combined.Ids.Select(id => plugin.UnlockablesService.GetUnlockableAchievement(id)).ToList();
            return [];
        }).ToList();

        if (Configuration.SortBy == SortBy.Alphabetically) {
            CategoryUnlockables.Sort((a, b) => string.Compare(a.NameLowercase(), b.NameLowercase(), StringComparison.OrdinalIgnoreCase));
        }
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
        SetCategory(SelectedCategoryId);
        CalculateAchievementPoints();
    }

    private AchievementLayoutCategory? FindCategory(IEnumerable<AchievementLayout> group, int id) {
        if (id == PinnedAchievementsCategoryId) return pinnedAchievementsCategory;
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

    public void RefreshPinnedAchievements() {
        pinnedAchievementsCategory = new AchievementLayoutCategory {
            Id = PinnedAchievementsCategoryId,
            Name = "Pinned",
            Items = plugin.Configuration.PinnedAchievements.Select(it => {
                var unlockable = plugin.UnlockablesService.GetExistingAchievement(it);
                AchievementLayoutItem? item = unlockable switch {
                    UnlockableAchievement => new AchievementLayoutItemSimple { Id = it },
                    UnlockableTieredAchievement tiered => new AchievementLayoutItemTiered { Ids = tiered.Ids(), Spoilers = tiered.Spoilers() },
                    _ => null,
                };
                return item;
            }).Where(it => it is not null).ToList()!,
        };
    }

    public void Refresh() {
        plugin.UnlockablesService.Refresh();
        FilterAll();
        CalculateAchievementPoints();
        RefreshPinnedAchievements();
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
        SelectedCategoryId = categoryId;
        SelectedAchievementCategory = FindCategory(FilteredLayout.AchievementLayout, categoryId);
        if (SelectedAchievementCategory == null) {
            SelectedCategoryId = NoCategoryId;
        }

        SortCategory();
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
        SortCategory();
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
        DebugStopwatch.Start();
    }

    public void DebugEnd() {
        if (!Configuration.DebugMode) return;
    }
}
