using System;
using System.Collections.Generic;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace BetterAchievements.UI.Component.Sidebar;

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

        if (CategoryRow("##Pinned", FontAwesomeIcon.Thumbtack, "Pinned", progress, null, UiColors.Progress(), isPinned, isPinned)) {
            state.Navigation.Navigate(target);
        }
    }

    private static void OverviewItem(MainWindowState state) {
        var target = new NavigationTarget.Overview();
        var isOverview = state.Navigation.IsSelected(target);
        var (obtained, total) = state.Unlockables.ComputeOverallProgress();
        var progress = total == 0 ? 0f : (float)obtained / total;

        if (CategoryRow("##Overview", FontAwesomeIcon.Home, "Overview", progress, null, UiColors.Progress(), isOverview, isOverview)) {
            state.Navigation.Navigate(target);
        }
    }

    private static void MainCategoryItem(MainWindowState state, AchievementLayout layout) {
        var (obtained, total) = state.Unlockables.ComputeProgress(layout);
        var progress = total == 0 ? 0f : (float)obtained / total;

        switch (layout) {
            case AchievementLayoutGroup group: {
                var target = new NavigationTarget.Group(group.Name);
                var isOpen = state.Navigation.IsGroupOpen(group.Name);
                var selected = state.Navigation.IsSelected(target);
                var icon = ParseIcon(group.Icon);
                var color = UiColors.Parse(group.Color);

                if (CategoryRow($"##MainCategory-{group.Name}", icon, group.Name, progress, color, UiColors.Progress(), isOpen, selected)) {
                    state.Navigation.Navigate(target);
                }

                if (isOpen) {
                    ImGui.Indent(UiSize.Em(FirstLevelIndentEm));
                    foreach (var item in group.Items) {
                        SubTree(state, item);
                    }

                    ImGui.Unindent(UiSize.Em(FirstLevelIndentEm));
                    ImGui.Dummy(new Vector2(0, UiSize.Em(SubTreeBottomPaddingEm)));
                }

                break;
            }

            case AchievementLayoutCategory category: {
                var target = new NavigationTarget.Category(category.Id);
                var selected = state.Navigation.IsSelected(target);

                if (CategoryRow($"##MainCategory-{category.Id}", FontAwesomeIcon.Star, category.Name, progress, null, UiColors.Progress(), selected, selected)) {
                    state.Navigation.Navigate(target);
                }

                break;
            }
        }
    }
}
