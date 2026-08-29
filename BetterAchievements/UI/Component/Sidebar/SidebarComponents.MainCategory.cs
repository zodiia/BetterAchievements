using System;
using System.Collections.Generic;
using BetterAchievements.Data;
using BetterAchievements.UI.Windows;
using BetterAchievements.UI.Windows.Views;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace BetterAchievements.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private const float FirstLevelIndentEm = 1.8f;

    private static readonly Dictionary<string, FontAwesomeIcon> IconCache = new();

    private static FontAwesomeIcon ParseIcon(string? name) {
        if (string.IsNullOrWhiteSpace(name)) return FontAwesomeIcon.Star;

        if (!IconCache.TryGetValue(name, out var icon)) {
            icon = Enum.TryParse<FontAwesomeIcon>(name, true, out var parsed) ? parsed : FontAwesomeIcon.Star;
            IconCache[name] = icon;
        }

        return icon;
    }

    private static void PinnedAchievementsItem(MainWindowState state) {
        var isPinned = state.SelectedCategoryId == MainWindowState.PinnedAchievementsCategoryId;
        var (obtained, total) = state.ComputeProgress(state.Configuration.PinnedAchievements);
        var progress = total == 0 ? 0f : (float)obtained / total;

        if (CategoryRow("##Pinned", FontAwesomeIcon.Thumbtack, "Pinned", progress, null, isPinned, isPinned)) {
            state.OpenPinnedAchievements();
        }
    }

    private static void OverviewItem(MainWindowState state) {
        var isOverview = state.CurrentView is OverviewView;
        var (obtained, total) = state.ComputeOverallProgress();
        var progress = total == 0 ? 0f : (float)obtained / total;

        if (CategoryRow("##Overview", FontAwesomeIcon.Home, "Overview", progress, null, isOverview, isOverview)) {
            state.OpenOverview();
        }
    }

    private static void MainCategoryItem(MainWindowState state, AchievementLayout layout) {
        var (obtained, total) = state.ComputeProgress(layout);
        var progress = total == 0 ? 0f : (float)obtained / total;

        switch (layout) {
            case AchievementLayoutGroup group: {
                var isOpen = state.OpenTopLevelGroupName == group.Name;
                var selected = isOpen && state.SelectedCategoryId == MainWindowState.NoCategoryId;
                var icon = ParseIcon(group.Icon);
                var color = UiColors.Parse(group.Color);

                if (CategoryRow($"##MainCategory-{group.Name}", icon, group.Name, progress, color, isOpen, selected)) {
                    state.OpenAchievementCategoryGroup(group);
                }

                if (isOpen) {
                    ImGui.Indent(UiSize.Em(FirstLevelIndentEm));
                    foreach (var item in group.Items) {
                        SubTree(state, item);
                    }

                    ImGui.Unindent(UiSize.Em(FirstLevelIndentEm));
                }

                break;
            }

            case AchievementLayoutCategory category: {
                var selected = state.SelectedCategoryId == category.Id;

                if (CategoryRow($"##MainCategory-{category.Id}", FontAwesomeIcon.Star, category.Name, progress, null, selected, selected)) {
                    state.SetCategory(category.Id);
                }

                break;
            }
        }
    }
}
