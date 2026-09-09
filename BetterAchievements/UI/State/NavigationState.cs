using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.Services;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.Windows.Views;
using BetterAchievements.UI.Windows.Views.Overview;

namespace BetterAchievements.UI.State;

public abstract record NavigationTarget {
    public sealed record Overview : NavigationTarget;
    public sealed record Pinned : NavigationTarget;
    public sealed record Category(int Id) : NavigationTarget;
    public sealed record Group(string Name) : NavigationTarget;
    public sealed record Collection(UnlockableType Type) : NavigationTarget;
    public sealed record CollectionCategory(UnlockableType Type, uint Id) : NavigationTarget;
    public sealed record Todo(string Name) : NavigationTarget;
}

public class NavigationState {
    private readonly Plugin plugin;
    private readonly UnlockablesState unlockables;
    private readonly Dictionary<NavigationTarget, VariableHeightClipper> clippers = new();

    public NavigationTarget Target { get; private set; } = new NavigationTarget.Overview();
    public string? OpenGroupName { get; private set; }
    public IView CurrentView { get; private set; }

    public NavigationState(Plugin plugin, UnlockablesState unlockables) {
        this.plugin = plugin;
        this.unlockables = unlockables;
        CurrentView = new OverviewView(plugin, unlockables, this);
    }

    private void SetNavigation(NavigationTarget target, string? openGroupName, IView view) {
        Target = target;
        OpenGroupName = openGroupName;
        CurrentView = view;
    }

    private VariableHeightClipper ClipperFor(NavigationTarget target) {
        if (!clippers.TryGetValue(target, out var clipper)) {
            clipper = new VariableHeightClipper();
            clippers[target] = clipper;
        }

        return clipper;
    }

    private CollectionView CollectionView(NavigationTarget target, UnlockableType type, CollectionCategory category, string breadcrumb) {
        return new CollectionView(
            plugin,
            type,
            breadcrumb,
            unlockables.SortedCollectionUnlockables(type, category),
            unlockables.ComputeProgress(type, category).Score,
            ClipperFor(target));
    }

    public bool IsSelected(NavigationTarget target) => Target == target;

    public bool IsGroupOpen(string name) => OpenGroupName == name;

    public void Navigate(NavigationTarget target) {
        switch (target) {
            case NavigationTarget.Category category:
                var found = unlockables.FindCategory(category.Id);
                if (found == null) {
                    // fall back to overview (shouldn't happen)
                    Navigate(new NavigationTarget.Overview());
                    return;
                }

                var view = new AchievementsView(
                    plugin,
                    found.Breadcrumb,
                    unlockables.SortedUnlockables(found.Category),
                    unlockables.ComputeProgress(found.Category),
                    unlockables.ComputeAchievementCount(found.Category),
                    ClipperFor(target));
                SetNavigation(target, found.Breadcrumb.Split(" / ")[0], view);
                break;

            case NavigationTarget.Group group:
                var layoutGroup = unlockables.FindTopLevelGroup(group.Name);
                if (layoutGroup == null) {
                    // fallback to overview (shouldn't happen)
                    Navigate(new NavigationTarget.Overview());
                    return;
                }

                SetNavigation(target, group.Name, new AchievementCategoryView(plugin, layoutGroup, unlockables, this));
                break;

            case NavigationTarget.Collection collection:
                var singleCategory = unlockables.CollectionCategories(collection.Type).FirstOrDefault();
                if (collection.Type == UnlockableType.Title && singleCategory == null) {
                    // fall back to overview (shouldn't happen)
                    Navigate(new NavigationTarget.Overview());
                    return;
                }

                IView collectionView = collection.Type == UnlockableType.Title
                    ? CollectionView(target, collection.Type, singleCategory!, CollectionsService.Label(collection.Type))
                    : new CollectionOverviewView(plugin, collection.Type, unlockables, this);

                SetNavigation(target, CollectionsService.Label(collection.Type), collectionView);
                break;

            case NavigationTarget.CollectionCategory collectionCategory:
                var collectionFound = unlockables.FindCollectionCategory(collectionCategory.Type, collectionCategory.Id);
                if (collectionFound == null) {
                    // fall back to overview (shouldn't happen)
                    Navigate(new NavigationTarget.Overview());
                    return;
                }

                SetNavigation(target, CollectionsService.Label(collectionCategory.Type),
                              CollectionView(target, collectionCategory.Type, collectionFound,
                                             $"{CollectionsService.Label(collectionCategory.Type)} / {collectionFound.Name}"));
                break;

            case NavigationTarget.Pinned:
                SetNavigation(target, null, new AchievementsView(
                    plugin,
                    "Pinned",
                    unlockables.PinnedUnlockables(),
                    unlockables.ComputeProgress(plugin.Configuration.PinnedAchievements),
                    unlockables.ComputeAchievementCount(plugin.Configuration.PinnedAchievements),
                    ClipperFor(target)));
                break;

            case NavigationTarget.Todo:
                SetNavigation(target, null, new TodoView(plugin));
                break;

            default:
                SetNavigation(new NavigationTarget.Overview(), null, new OverviewView(plugin, unlockables, this));
                break;
        }
    }

    public void Rebuild() => Navigate(Target);

}
