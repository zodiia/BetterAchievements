using System.Numerics;
using BetterAchievements.UI.Windows;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private const float SectionPaddingAboveEm = 0.5f;
    private const float SectionPaddingBelowEm = 0.25f;

    private static void SectionHeader(string name) {
        UiComponents.SeparatorText(name, UiFonts.FontSize100, UiColors.Grey(), SectionPaddingAboveEm, SectionPaddingBelowEm);
    }

    private static void FillerItems(params string[] names) {
        foreach (var name in names) {
            ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
        }
    }

    public static void Sidebar(MainWindowState state, float sidebarWidth) {
        var ySize = ImGui.GetContentRegionAvail().Y - (state.Configuration.DebugMode ? 32 : 0);
        using var sidebar = ImRaii.Child("Sidebar", new Vector2 { X = sidebarWidth, Y = ySize }, true, ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!sidebar) return;

        SectionHeader("Achievements");
        PinnedAchievementsItem(state);
        OverviewItem(state);
        foreach (var layout in state.FilteredLayout.AchievementLayout) {
            MainCategoryItem(state, layout);
        }

        SectionHeader("Collections");
        FillerItems("Mounts", "Minions", "Titles", "Triple Triad Cards", "Triple Triad NPCs", "Orchestrion Rolls", "Bardings", "Fashion Accessories");

        SectionHeader("Records");
        FillerItems("Duty Records", "PvP Records", "Gold Saucer Records", "Character Records");

        SectionHeader("Seasonal & Others");
        FillerItems("Seasonal Events", "Anniversary", "Fan Festival", "Legacy");
    }
}
