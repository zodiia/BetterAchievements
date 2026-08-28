using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.UI.Windows;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents {
    private static void PinnedAchievementsSidebarItem(MainWindowState state) {
        if (ImGui.TreeNodeEx("Pinned", ImGuiTreeNodeFlags.Leaf |
                                       ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                       ImGuiTreeNodeFlags.SpanFullWidth |
                                       (state.SelectedCategoryId == MainWindowState.PinnedAchievementsCategoryId ? ImGuiTreeNodeFlags.Selected : 0))) {
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
                state.OpenPinnedAchievements();
            }
        }
    }

    private static void SidebarItem(MainWindowState state, string name, int categoryId) {
        if (ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.Leaf |
                                   ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                   ImGuiTreeNodeFlags.SpanFullWidth |
                                   (state.SelectedCategoryId == categoryId ? ImGuiTreeNodeFlags.Selected : 0))) {
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
                state.SetCategory(categoryId);
            }
        }
    }

    private static void SidebarAchievementLayout(MainWindowState state, AchievementLayout layout) {
        switch (layout) {
            case AchievementLayoutGroup group:
                if (ImGui.TreeNodeEx(group.Name, ImGuiTreeNodeFlags.SpanFullWidth)) {
                    foreach (var subLayout in group.Items) {
                        SidebarAchievementLayout(state, subLayout);
                    }

                    ImGui.TreePop();
                }

                break;

            case AchievementLayoutCategory category:
                SidebarItem(state, category.Name, category.Id);

                break;
        }
    }

    public static void Sidebar(MainWindowState state, float sidebarWidth) {
        var ySize = ImGui.GetContentRegionAvail().Y - (state.Configuration.DebugMode ? 32 : 0);
        using var sidebar = ImRaii.Child("Sidebar", new Vector2 { X = sidebarWidth, Y = ySize }, true);
        if (!sidebar) return;

        if (ImGui.CollapsingHeader("Achievements")) {
            PinnedAchievementsSidebarItem(state);

            foreach (var layout in state.FilteredLayout.AchievementLayout) {
                SidebarAchievementLayout(state, layout);
            }
        }

        if (ImGui.CollapsingHeader("Fishing")) {
            ImGui.TreeNodeEx("Fish Guide", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Spearfish Guide", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            using var regions = ImRaii.TreeNode("Regions", ImGuiTreeNodeFlags.SpanFullWidth);
            if (regions.Success) {
                using var region = ImRaii.TreeNode("La Noscea", ImGuiTreeNodeFlags.SpanFullWidth);
                if (region.Success) {
                    using var area = ImRaii.TreeNode("Middle La Noscea", ImGuiTreeNodeFlags.SpanFullWidth);
                    if (area.Success) {
                        ImGui.TreeNodeEx("Zephyr Drift", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("Summerfold", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("Rogue River", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("West Agelyss River",
                                         ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("Nym River", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("Woad Whisper Canyon",
                                         ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                    }
                }
            }
        }

        if (ImGui.CollapsingHeader("Collectibles")) {
            ImGui.TreeNodeEx("Mounts", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Minions", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Titles", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Triple Triad Cards", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Triple Triad NPCs", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Orchestrion Rolls", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Portraits", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Levequests", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Bardings", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Emotes", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Fashion Accessories", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
        }
    }
}
