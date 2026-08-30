using System;
using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.Helpers;
using BetterAchievements.Services;

namespace BetterAchievements.UI.State;

public class UnlockablesState(Plugin plugin) {
    private readonly Configuration configuration = plugin.Configuration;
    private readonly MainLayout mainLayout = plugin.MainLayout;
    private readonly Dictionary<AchievementLayout, PointsScore> progressCache = new(ReferenceEqualityComparer.Instance);

    private string search = "";
    private ulong achievementArrayHash;

    public MainLayout FilteredLayout { get; private set; } = plugin.MainLayout;
    public PointsScore AchievementPoints { get; private set; } = new(0, 0);

    public void SetSearch(string value) {
        search = value.ToLower();
        ApplyFilters();
    }

    public void ApplyFilters() {
        var items = mainLayout.AchievementLayout.Select(FilterAchievementLayout).OfType<AchievementLayout>().ToList();

        FilteredLayout = new MainLayout { AchievementLayout = items };
        progressCache.Clear();
    }

    public void Refresh() {
        plugin.UnlockablesService.Refresh();
        ApplyFilters();
        AchievementPoints = UnlockablesService.CalculateAchievementPoints();
    }

    public unsafe bool CheckForUpdates() {
        var newHash = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement.Instance()->CompletedAchievementsBitArray.ComputeHash();
        if (newHash.Equals(achievementArrayHash) && !plugin.UnlockablesProgressService.CheckUpdated()) {
            return false;
        }

        achievementArrayHash = newHash;
        Refresh();
        return true;
    }

    public PointsScore ComputeProgress(IEnumerable<uint> achievementIds) {
        return plugin.UnlockablesService.CalculateAchievementPoints(achievementIds);
    }

    public PointsScore ComputeProgress(AchievementLayout layout) {
        if (progressCache.TryGetValue(layout, out var cached)) return cached;

        var result = ComputeProgress(layout.GetAllAchievementIds());
        progressCache[layout] = result;
        return result;
    }

    public PointsScore ComputeOverallProgress() {
        uint obtained = 0;
        uint total = 0;

        foreach (var layout in FilteredLayout.AchievementLayout) {
            var (layoutObtained, layoutTotal) = ComputeProgress(layout);
            obtained += layoutObtained;
            total += layoutTotal;
        }

        return new PointsScore(obtained, total);
    }

    public (AchievementLayoutCategory Category, string Breadcrumb)? FindCategory(int id) {
        return FindCategory(FilteredLayout.AchievementLayout, id);
    }

    public AchievementLayoutGroup? FindTopLevelGroup(string name) {
        return FilteredLayout.AchievementLayout.OfType<AchievementLayoutGroup>().FirstOrDefault(it => it.Name == name);
    }

    public List<IUnlockable> SortedUnlockables(AchievementLayoutCategory category) {
        var items = category.Items;

        if (configuration.SortBy is SortBy.MostCommon or SortBy.Rarest) {
            items = items.OrderBy(Rarity).ToList();
            if (configuration.SortBy == SortBy.MostCommon) items.Reverse();
        }

        var unlockables = items.SelectMany<AchievementLayoutItem, IUnlockable>(it => {
            return it switch {
                AchievementLayoutItemSimple simple => [
                    plugin.UnlockablesService.GetUnlockableAchievement(simple.Id)
                ],
                AchievementLayoutItemTiered tiered => [
                    plugin.UnlockablesService.GetUnlockableTieredAchievement(tiered.Ids, tiered.Spoilers)
                ],
                _ => []
            };
        }).ToList();

        if (configuration.SortBy == SortBy.Alphabetically) {
            unlockables.Sort((a, b) => string.Compare(a.NameLowercase(), b.NameLowercase(), StringComparison.OrdinalIgnoreCase));
        }

        return unlockables;
    }

    public List<IUnlockable> PinnedUnlockables() {
        return plugin.UnlockablesService.GetPinnedUnlockables();
    }

    private static (AchievementLayoutCategory Category, string Breadcrumb)? FindCategory(IEnumerable<AchievementLayout> group, int id, string prefix = "") {
        foreach (var item in group) {
            switch (item) {
                case AchievementLayoutGroup subgroup:
                    var res = FindCategory(subgroup.Items, id, prefix.Length == 0 ? subgroup.Name : $"{prefix} / {subgroup.Name}");
                    if (res != null) {
                        return res;
                    }

                    break;
                case AchievementLayoutCategory category when category.Id == id:
                    return (category, prefix.Length == 0 ? category.Name : $"{prefix} / {category.Name}");
            }
        }

        return null;
    }

    private uint Rarity(AchievementLayoutItem item) => item switch {
        AchievementLayoutItemSimple simple => plugin.LalachievementsService.AchievementRarity.GetValueOrDefault(simple.Id, uint.MaxValue),
        AchievementLayoutItemTiered tiered => plugin.LalachievementsService.AchievementRarity.GetValueOrDefault(tiered.Ids.Last(), uint.MaxValue),
        _ => uint.MaxValue
    };

    private bool MatchSearch(string name, string desc) => name.Contains(search) || desc.Contains(search);

    private bool MatchUnlockFilter(bool unlocked) =>
        configuration.UnlockStatusFilter switch {
            UnlockStatusFilter.All => true,
            UnlockStatusFilter.Unlocked => unlocked,
            UnlockStatusFilter.Locked => !unlocked,
            _ => throw new ArgumentOutOfRangeException($"{configuration.UnlockStatusFilter} not implemented.")
        };

    private bool MatchRankedFilter(bool lalachievements) => configuration.RankedFilter switch {
        RankedFilter.All => true,
        RankedFilter.Lalachievements => lalachievements,
        _ => throw new ArgumentOutOfRangeException($"{configuration.RankedFilter} not implemented.")
    };

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

    private bool FilterAchievementLayoutItem(AchievementLayoutItem item) => item switch {
        AchievementLayoutItemSimple simple => FilterAchievementLayoutItem(simple),
        AchievementLayoutItemTiered tiered => FilterAchievementLayoutItem(tiered),
        _ => false
    };

    private AchievementLayoutCategory? FilterAchievementLayout(AchievementLayoutCategory category) {
        var items = category.Items.Where(FilterAchievementLayoutItem).ToList();
        if (items.Count == 0) return null;
        return category with { Items = items };
    }

    private AchievementLayoutGroup? FilterAchievementLayout(AchievementLayoutGroup group) {
        var items = group.Items.Select(FilterAchievementLayout).OfType<AchievementLayout>().ToList();
        if (items.Count == 0) return null;
        return group with { Items = items };
    }

    private AchievementLayout? FilterAchievementLayout(AchievementLayout layout) => layout switch {
        AchievementLayoutGroup group => FilterAchievementLayout(group),
        AchievementLayoutCategory category => FilterAchievementLayout(category),
        _ => null
    };
}
