using System;
using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Unlockables;
using BetterAchievements.Windows.Helpers;

namespace BetterAchievements.Windows;

public class MainWindowState(MainWindowLayout layout)
{
    private readonly MainWindowLayout mainLayout = layout;

    private uint currentCategoryId = uint.MaxValue;
    private string currentSearch = "";

    public MainWindowLayout FilteredLayout { get; private set; } = layout;
    public AchievementLayoutCategory? SelectedCategory { get; private set; }
    public List<Unlockable> CategoryUnlockables { get; private set; } = new();
    public string SearchBuffer = "";
    public UnlockStatusFilter UnlockStatusFilter { get; private set; } = UnlockStatusFilter.All;
    public ContainsRewardsFilter ContainsRewardsFilter { get; private set; } = ContainsRewardsFilter.All;
    public RankedFilter RankedFilter { get; private set; } = RankedFilter.All;
    public AreaFilter AreaFilter { get; private set; } = AreaFilter.All;
    public SortBy SortBy { get; private set; } = SortBy.Default;
    public GroupBy GroupBy { get; private set; } = GroupBy.Default;

    private bool MatchSearch(string name, string desc)
    {
        return name.Contains(currentSearch) || desc.Contains(currentSearch);
    }

    private bool MatchUnlockFilter(bool unlocked)
    {
        switch (UnlockStatusFilter)
        {
            case UnlockStatusFilter.All: return true;
            case UnlockStatusFilter.Unlocked: return unlocked;
            case UnlockStatusFilter.Locked: return !unlocked;
        }
        throw new ArgumentOutOfRangeException($"{UnlockStatusFilter} not implemented.");
    }

    private bool MatchRankedFilter(bool lalachievements)
    {
        switch (RankedFilter)
        {
            case RankedFilter.All: return true;
            case RankedFilter.Lalachievements: return lalachievements;
        }
        throw new ArgumentOutOfRangeException($"{RankedFilter} not implemented.");
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemSimple item)
    {
        var achievement = Plugin.UnlockablesService.GetUnlockableAchievement(item.Id);
        return MatchSearch(achievement.NameLowercase(), achievement.DescriptionLowercase())
               && MatchUnlockFilter(achievement.Unlocked)
               && MatchRankedFilter(Plugin.LalachievementsService.AchievementRarity.ContainsKey(achievement.Id()));
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemTiered item)
    {
        var achievements = Plugin.UnlockablesService.GetUnlockableTieredAchievement(item.Ids);
        return MatchSearch(achievements.NameLowercase(), achievements.DescriptionLowercase())
               && MatchUnlockFilter(achievements.Unlocked.Last())
               && MatchRankedFilter(Plugin.LalachievementsService.AchievementRarity.ContainsKey(achievements.Achievements.Last().RowId));
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemCombined item)
    {
        var achievements = Plugin.UnlockablesService.GetUnlockableAchievement(item.Ids.Last());
        return MatchSearch(achievements.NameLowercase(), achievements.DescriptionLowercase())
               && MatchRankedFilter(Plugin.LalachievementsService.AchievementRarity.ContainsKey(achievements.Id()));
        // && MatchUnlockFilter(achievements.Unlocked.All());
        // TODO
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItem item)
    {
        if (item is AchievementLayoutItemSimple simple) return FilterAchievementLayoutItem(simple);
        if (item is AchievementLayoutItemCombined combined) return FilterAchievementLayoutItem(combined);
        if (item is AchievementLayoutItemTiered tiered) return FilterAchievementLayoutItem(tiered);
        return false;
    }

    private AchievementLayoutCategory? FilterAchievementLayout(AchievementLayoutCategory category)
    {
        var items = category.Items.Where(FilterAchievementLayoutItem).ToList();
        if (items.Count == 0) return null;
        return category with { Items = items };
    }

    private AchievementLayoutGroup? FilterAchievementLayout(AchievementLayoutGroup group)
    {
        var items = group.Items.SelectMany<AchievementLayout, AchievementLayout>(it =>
        {
            var res = FilterAchievementLayout(it);
            if (res == null) return [];
            return [res];
        }).ToList();
        if (items.Count == 0) return null;
        return group with { Items = items };
    }

    private AchievementLayout? FilterAchievementLayout(AchievementLayout layout)
    {
        if (layout is AchievementLayoutGroup group) return FilterAchievementLayout(group);
        if (layout is AchievementLayoutCategory category) return FilterAchievementLayout(category);
        return null;
    }

    private void SortCategory()
    {
        if (SelectedCategory == null) return;
        List<AchievementLayoutItem> sortedItems;

        if (SortBy is SortBy.MostCommon or SortBy.Rarest)
        {
            sortedItems = SelectedCategory.Items.OrderBy(it =>
            {
                if (it is AchievementLayoutItemSimple simple)
                    return Plugin.LalachievementsService.AchievementRarity.GetValueOrDefault(simple.Id, uint.MaxValue);
                if (it is AchievementLayoutItemTiered tiered)
                    return Plugin.LalachievementsService.AchievementRarity.GetValueOrDefault(tiered.Ids.Last(), uint.MaxValue);
                if (it is AchievementLayoutItemCombined combined) // TODO: fix
                    return Plugin.LalachievementsService.AchievementRarity.GetValueOrDefault(combined.Ids.Last(), uint.MaxValue);
                return uint.MaxValue;
            }).ToList();
            if (SortBy == SortBy.MostCommon)
            {
                sortedItems.Reverse();
            }
        }
        else
        {
            sortedItems = SelectedCategory.Items;
        }

        CategoryUnlockables = sortedItems.SelectMany<AchievementLayoutItem, Unlockable>(it =>
        {
            if (it is AchievementLayoutItemSimple simple)
                return [Plugin.UnlockablesService.GetUnlockableAchievement(simple.Id)];
            if (it is AchievementLayoutItemTiered tiered)
                return [Plugin.UnlockablesService.GetUnlockableTieredAchievement(tiered.Ids)];
            if (it is AchievementLayoutItemCombined combined) // TODO: fix
                return combined.Ids.Select(simple => Plugin.UnlockablesService.GetUnlockableAchievement(simple)).ToList();
            return [];
        }).ToList();

        if (SortBy == SortBy.Alphabetically)
        {
            CategoryUnlockables.Sort((a, b) => string.Compare(a.NameLowercase(), b.NameLowercase(), StringComparison.OrdinalIgnoreCase));
        }
    }

    private void FilterAll()
    {
        var items = mainLayout.AchievementLayout.SelectMany<AchievementLayout, AchievementLayout>(it =>
        {
            var res = FilterAchievementLayout(it);
            if (res == null) return [];
            return [res];
        }).ToList();
        FilteredLayout = new MainWindowLayout { AchievementLayout = items };
        SetCategory(currentCategoryId);
    }

    private AchievementLayoutCategory? FindCategory(IEnumerable<AchievementLayout> group, uint id)
    {
        foreach (var item in group)
        {
            if (item is AchievementLayoutGroup subgroup)
            {
                var res = FindCategory(subgroup.Items, id);
                if (res != null)
                {
                    return res;
                }
            }
            if (item is AchievementLayoutCategory category)
            {
                if (category.Id == id) return category;
            }
        }
        return null;
    }

    public void SetCategory(uint categoryId)
    {
        currentCategoryId = categoryId;
        SelectedCategory = FindCategory(FilteredLayout.AchievementLayout, categoryId);
        SortCategory();
    }

    public void SetSearch(string search)
    {
        currentSearch = search.ToLower();
        FilterAll();
    }

    public void SetUnlockStatusFilter(UnlockStatusFilter unlockStatusFilter)
    {
        UnlockStatusFilter = unlockStatusFilter;
        FilterAll();
    }

    public void SetContainsRewardsFilter(ContainsRewardsFilter containsRewardsFilter)
    {
        ContainsRewardsFilter = containsRewardsFilter;
        FilterAll();
    }

    public void SetRankedFilter(RankedFilter rankedFilter)
    {
        RankedFilter = rankedFilter;
        FilterAll();
    }

    public void SetAreaFilter(AreaFilter areaFilter)
    {
        AreaFilter = areaFilter;
        FilterAll();
    }

    public void SetSortBy(SortBy sortBy)
    {
        SortBy = sortBy;
        SortCategory();
    }

    public void SetGroupBy(GroupBy groupBy)
    {
        GroupBy = groupBy;
    }
}
