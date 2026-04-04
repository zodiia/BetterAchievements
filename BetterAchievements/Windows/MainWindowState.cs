using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Unlockables;

namespace BetterAchievements.Windows;

public class MainWindowState(MainWindowLayout layout)
{
    private readonly UnlockablesService unlockablesService = Plugin.UnlockablesService;

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

    private bool FilterAchievementLayoutItem(AchievementLayoutItemSimple item)
    {
        var achievement = unlockablesService.GetUnlockableAchievement(item.Id);
        return MatchSearch(achievement.NameLowercase(), achievement.DescriptionLowercase())
               && MatchUnlockFilter(achievement.Unlocked);
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemTiered item)
    {
        var achievements = unlockablesService.GetUnlockableTieredAchievement(item.Ids);
        return MatchSearch(achievements.NameLowercase(), achievements.DescriptionLowercase())
               && MatchUnlockFilter(achievements.Unlocked.Last());
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemCombined item)
    {
        var achievements = unlockablesService.GetUnlockableAchievement(item.Ids.Last());
        return MatchSearch(achievements.NameLowercase(), achievements.DescriptionLowercase());
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

    private void FilterAll()
    {
        var items = mainLayout.AchievementLayout.SelectMany<AchievementLayout, AchievementLayout>(it =>
        {
            var res = FilterAchievementLayout(it);
            if (res == null) return [];
            return [res];
        }).ToList();
        FilteredLayout = mainLayout with { AchievementLayout = items };
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
    }

    public void SetGroupBy(GroupBy groupBy)
    {
        GroupBy = groupBy;
    }
}
