using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Helpers;
using BetterAchievements.Unlockables;
using Serilog;

namespace BetterAchievements.Windows;

public class MainWindowState(MainWindowLayout layout)
{
    private readonly UnlockablesService unlockablesService = Plugin.UnlockablesService;

    private readonly MainWindowLayout mainLayout = layout;

    private uint currentCategoryId = uint.MaxValue;
    private string currentSearch = "";

    public MainWindowLayout FilteredLayout { get; private set; } = layout;
    public AchievementLayoutCategory? SelectedCategory { get; private set; }
    public TriState FilterLocked { get; private set; } = TriState.Undefined;
    public TriState FilterHasRewards { get; private set; } = TriState.Undefined;
    public TriState FilterIsRanked { get; private set; } = TriState.Undefined;
    public TriState FilterCurrentZone { get; private set; } = TriState.Undefined;
    public string SearchBuffer = "";

    private bool FilterAchievementLayoutItem(AchievementLayoutItemSimple item)
    {
        var achievement = unlockablesService.GetUnlockableAchievement(item.Id);
        return achievement.NameLowercase.Contains(currentSearch) || achievement.DescriptionLowercase.Contains(currentSearch);
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemTiered item)
    {
        var achievements = unlockablesService.GetUnlockableTieredAchievement(item.Ids);
        return achievements.NameLowercase.Contains(currentSearch) || achievements.DescriptionLowercase.Contains(currentSearch);
    }

    private bool FilterAchievementLayoutItem(AchievementLayoutItemCombined item)
    {
        var achievements = unlockablesService.GetUnlockableAchievement(item.Ids.Last());
        return achievements.NameLowercase.Contains(currentSearch) || achievements.DescriptionLowercase.Contains(currentSearch);
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
        Log.Information("{Count}", items.Count);
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

    public void SetFilterLocked(TriState state)
    {
        FilterLocked = state;
    }

    public void SetFilterHasRewards(TriState state)
    {
        FilterHasRewards = state;
    }

    public void SetFilterIsRanked(TriState state)
    {
        FilterIsRanked = state;
    }

    public void SetFilterCurrentZone(TriState state)
    {
        FilterCurrentZone = state;
    }
}
