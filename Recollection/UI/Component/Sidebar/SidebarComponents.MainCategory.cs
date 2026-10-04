using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Recollection.Data;
using Recollection.UI.State;

namespace Recollection.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private const float FirstLevelIndentEm = 1.8f;
    private const float SubTreeBottomPaddingEm = 1f;

    private static readonly Dictionary<string, FontAwesomeIcon> IconCache = new();

    private static FontAwesomeIcon ParseIcon(string? name) {
        if (string.IsNullOrWhiteSpace(name)) return FontAwesomeIcon.Star;

        if (!IconCache.TryGetValue(name, out var icon)) {
            icon = Enum.TryParse<FontAwesomeIcon>(name, true, out var parsed) ? parsed : FontAwesomeIcon.Star;
            IconCache[name] = icon;
        }

        return icon;
    }

    private static void PinnedAchievementsItem(Plugin plugin, MainWindowState state) {
        var target = new NavigationTarget.Pinned();
        var isPinned = state.Navigation.IsSelected(target);
        var (obtained, total) = state.Unlockables.ComputeProgress(plugin.Configuration.PinnedAchievements);
        var progress = total == 0 ? 0f : (float)obtained / total;

        if (CategoryRow(plugin, "##Pinned", FontAwesomeIcon.Thumbtack, UiColors.Progress(), "Pinned", progress, isPinned, isPinned)) {
            state.Navigation.Navigate(target);
        }
    }

    private static void OverviewItem(Plugin plugin, MainWindowState state) {
        var target = new NavigationTarget.Overview();
        var isOverview = state.Navigation.IsSelected(target);
        var (obtained, total) = state.Unlockables.ComputeOverallProgress();
        var progress = total == 0 ? 0f : (float)obtained / total;

        if (CategoryRow(plugin, "##Overview", FontAwesomeIcon.Home, UiColors.Progress(), "Overview", progress, isOverview, isOverview)) {
            state.Navigation.Navigate(target);
        }
    }

    private static void MainCategoryItem(Plugin plugin, MainWindowState state, AchievementLayout layout) {
        var (obtained, total) = state.Unlockables.ComputeProgress(layout);
        var progress = total == 0 ? 0f : (float)obtained / total;

        switch (layout) {
            case AchievementLayoutGroup group: {
                var target = new NavigationTarget.Group(group.Name);
                var isOpen = state.Navigation.IsGroupOpen(group.Name);
                var selected = state.Navigation.IsSelected(target);
                var icon = ParseIcon(group.Icon);

                if (CategoryRow(plugin, $"##MainCategory-{group.Name}", icon, UiColors.Progress(), group.Name, progress, isOpen, selected)) {
                    state.Navigation.Navigate(target);
                }

                if (isOpen) {
                    ImGui.Indent(UiSize.Em(FirstLevelIndentEm * plugin.Configuration.UiDensity));
                    foreach (var item in group.Items) {
                        SubTree(plugin, state, item);
                    }

                    ImGui.Unindent(UiSize.Em(FirstLevelIndentEm * plugin.Configuration.UiDensity));
                    ImGui.Dummy(new Vector2(0, UiSize.Em(SubTreeBottomPaddingEm * plugin.Configuration.UiDensity)));
                }

                break;
            }

            case AchievementLayoutCategory category: {
                var target = new NavigationTarget.Category(category.Id);
                var selected = state.Navigation.IsSelected(target);

                if (CategoryRow(plugin, $"##MainCategory-{category.Id}", FontAwesomeIcon.Star, UiColors.Progress(), category.Name, progress, selected, selected)) {
                    state.Navigation.Navigate(target);
                }

                break;
            }
        }
    }
}
