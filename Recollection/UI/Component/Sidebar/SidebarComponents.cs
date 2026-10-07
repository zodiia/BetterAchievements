using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility.Numerics;
using Recollection.Data.Unlockable;
using Recollection.UI.State;
using Recollection.UI.Windows;

namespace Recollection.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private const float SectionPaddingAboveEm = 0.5f;
    private const float SectionPaddingBelowEm = 0.25f;

    private static void SectionHeader(Plugin plugin, string name) {
        UiComponents.SeparatorText(name, plugin.Configuration.UiDensity, UiFonts.FontSize100, UiColors.Grey(), SectionPaddingAboveEm, SectionPaddingBelowEm);
    }

    private static void FillerItems(Plugin plugin, MainWindowState state, params string[] names) {
        foreach (var name in names) {
            var target = new NavigationTarget.Todo(name);
            var selected = state.Navigation.IsSelected(target);
            if (CategoryRow(plugin, $"##Filler-{name}", FontAwesomeIcon.Lock, UiColors.Red(), name, 1.0f, selected, selected)) {
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
        if (ImGui.InputTextEx("##Search", "Search...", ref state.SearchBuffer, 128, new Vector2(searchWidth, 0))) {
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
        var ySize = ImGui.GetContentRegionAvail().Y;
        using var backgroundColor = ImRaii.PushColor(ImGuiCol.ChildBg, BackgroundColor());
        using var sidebar = ImRaii.Child("Sidebar", new Vector2 { X = sidebarWidth, Y = ySize }, true, ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!sidebar) return;

        SearchAndSettings(plugin, state);

        SectionHeader(plugin, "Achievements");
        PinnedAchievementsItem(plugin, state);
        OverviewItem(plugin, state);
        foreach (var layout in state.Unlockables.FilteredLayout.Achievements) {
            MainCategoryItem(plugin, state, layout);
        }

        SectionHeader(plugin, "Collections");
        if (!state.Unlockables.CollectionsLoaded) CollectionsLoading();
        CollectionItem(plugin, state, UnlockableType.Mount, UiColors.Blue());
        CollectionItem(plugin, state, UnlockableType.Minion, UiColors.Blue());
        CollectionItem(plugin, state, UnlockableType.Title, UiColors.Blue());
        CollectionItem(plugin, state, UnlockableType.OrchestrionRoll, UiColors.Blue());
        FillerItems(plugin, state, "Fishing");
        CollectionItem(plugin, state, UnlockableType.TripleTriadCard, UiColors.Blue());
        CollectionItem(plugin, state, UnlockableType.TripleTriadNpc, UiColors.Blue());
        CollectionItem(plugin, state, UnlockableType.Barding, UiColors.Blue());
        CollectionItem(plugin, state, UnlockableType.FashionAccessory, UiColors.Blue());
        CollectionItem(plugin, state, UnlockableType.Hairstyle, UiColors.Blue());
        CollectionItem(plugin, state, UnlockableType.Facewear, UiColors.Blue());
        CollectionItem(plugin, state, UnlockableType.Emote, UiColors.Blue());
        FillerItems(plugin, state, "Framer's Kits");
        CollectionItem(plugin, state, UnlockableType.Leve, UiColors.Blue());

        SectionHeader(plugin, "Records");
        CollectionItem(plugin, state, UnlockableType.HuntingLog, UiColors.Green());
        CollectionItem(plugin, state, UnlockableType.CraftingLog, UiColors.Green());
        CollectionItem(plugin, state, UnlockableType.GatheringLog, UiColors.Green());
        FillerItems(plugin, state, "Mount Speed", "Aether Currents", "Field Records", "Survey Records", "Occult Records");

        SectionHeader(plugin, "Seasonal & Others");
        FillerItems(plugin, state, "Yo-kai Watch", "The Rising");
    }
}
