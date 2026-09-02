using System;
using System.Numerics;
using BetterAchievements.UI.State;
using BetterAchievements.UI.Windows;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility.Numerics;

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
            var target = new NavigationTarget.Todo(name);
            var selected = state.Navigation.IsSelected(target);
            if (CategoryRow($"##Filler-{name}", FontAwesomeIcon.Lock, name, FillerProgressFor(name), null, defaultColor, selected, selected)) {
                state.Navigation.Navigate(target);
            }
        }
    }

    private static Vector4 BackgroundColor() {
        return UiColors.Text().WithW(0.03f);
    }

    private static void SearchAndSettings(Plugin plugin, MainWindowState state) {
        var style = ImGui.GetStyle();

        Vector2 settingsButtonSize;
        using (ImRaii.PushFont(UiBuilder.IconFont)) {
            settingsButtonSize = new Vector2(ImGui.CalcTextSize(FontAwesomeIcon.SlidersH.ToIconString()).X + (style.FramePadding.X * 2), ImGui.GetFrameHeight());
        }

        var searchWidth = Math.Max(ImGui.GetContentRegionAvail().X - settingsButtonSize.X - style.ItemSpacing.X, 0f);
        if (ImGui.InputTextEx("##Search", "Search achievements", ref state.SearchBuffer, 128, new Vector2(searchWidth, 0))) {
            state.SetSearch(state.SearchBuffer);
        }

        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.SlidersH)) {
            ImGui.OpenPopup(ConfigPopup.FiltersPopupId);
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Settings");
        }

        ConfigPopup.FiltersPopup(plugin, state);
    }

    public static void Sidebar(Plugin plugin, MainWindowState state, float sidebarWidth) {
        var ySize = UiSize.MainContentHeight(plugin.Configuration);
        using var backgroundColor = ImRaii.PushColor(ImGuiCol.ChildBg, BackgroundColor());
        using var sidebar = ImRaii.Child("Sidebar", new Vector2 { X = sidebarWidth, Y = ySize }, true, ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!sidebar) return;

        SearchAndSettings(plugin, state);

        SectionHeader("Achievements");
        PinnedAchievementsItem(plugin, state);
        OverviewItem(state);
        foreach (var layout in state.Unlockables.FilteredLayout.AchievementLayout) {
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
