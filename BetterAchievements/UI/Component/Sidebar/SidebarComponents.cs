using System;
using System.Numerics;
using BetterAchievements.UI.Windows;
using BetterAchievements.UI.Windows.Views;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility.Numerics;
using Serilog;

namespace BetterAchievements.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private const float SectionPaddingAboveEm = 0.5f;
    private const float SectionPaddingBelowEm = 0.25f;

    private static void SectionHeader(string name, float paddingAbove = SectionPaddingAboveEm, float paddingBelow = SectionPaddingBelowEm) {
        UiComponents.SeparatorText(name, UiFonts.FontSize100, UiColors.Grey(), paddingAbove, paddingBelow);
    }

    private static float FillerProgressFor(string name) {
        var hash = 17;
        unchecked {
            foreach (var c in name) hash = (hash * 31) + c;
        }

        return (float)new Random(hash).NextDouble();
    }

    private static void FillerItems(MainWindowState state, Vector4 defaultColor, params string[] names) {
        foreach (var name in names) {
            var selected = state.CurrentView is TodoView todo && todo.Name == name;
            if (CategoryRow($"##Filler-{name}", FontAwesomeIcon.Lock, name, FillerProgressFor(name), null, defaultColor, selected, selected)) {
                state.OpenTodo(name);
            }
        }
    }

    private static Vector4 BackgroundColor() {
        return UiColors.Text().WithW(0.03f);
    }

    public static void Sidebar(MainWindowState state, float sidebarWidth) {
        var ySize = ImGui.GetContentRegionAvail().Y - (state.Configuration.DebugMode ? 32 : 0);
        using var backgroundColor = ImRaii.PushColor(ImGuiCol.ChildBg, BackgroundColor());
        using var sidebar = ImRaii.Child("Sidebar", new Vector2 { X = sidebarWidth, Y = ySize }, true, ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!sidebar) return;

        SectionHeader("Achievements", 0f);
        PinnedAchievementsItem(state);
        OverviewItem(state);
        foreach (var layout in state.FilteredLayout.AchievementLayout) {
            MainCategoryItem(state, layout);
        }

        SectionHeader("Collections");
        FillerItems(state, UiColors.Blue(), "Mounts", "Minions", "Titles", "Fishing", "Triple Triad Cards", "Triple Triad NPCs", "Orchestrion Rolls", "Bardings", "Fashion Accessories", "Hairstyles", "Emotes", "Framer's Kits");

        SectionHeader("Records");
        FillerItems(state, UiColors.Green(), "Challenge Log", "Wondrous Tales", "Hunting Log", "Crafting Log", "Gathering Log", "Shared FATEs", "Mount Speed", "Aether Currents", "Field Records", "Survey Records", "Occult Records");

        SectionHeader("Seasonal & Others");
        FillerItems(state, UiColors.Red(), "Yo-kai Watch", "The Rising");
    }
}
