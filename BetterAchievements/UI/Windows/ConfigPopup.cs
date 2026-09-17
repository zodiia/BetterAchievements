using System;
using BetterAchievements.Data;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Windows;

public static class ConfigPopup {
    public const string FiltersPopupId = "FilterPopup";

    private static void EnumCombo<T>(string label, string preview, MainWindowState state, T current, Func<T, string> displayName,
                                     Action<Configuration, T> apply, ConfigurationEffect effect) where T : struct, Enum {
        ImGui.Text(label);

        using var combo = ImRaii.Combo("", preview, ImGuiComboFlags.HeightLargest);
        if (!combo) return;

        foreach (var value in Enum.GetValues<T>()) {
            if (ImGui.Selectable(displayName(value), current.Equals(value))) {
                state.UpdateConfiguration(configuration => apply(configuration, value), effect);
            }
        }
    }

    public static void FiltersPopup(Plugin plugin, MainWindowState state) {
        using var popupBackground = ImRaii.PushColor(ImGuiCol.PopupBg, UiColors.PopupBackground() with { W = 1f });
        using var popup = ImRaii.Popup(FiltersPopupId, ImGuiWindowFlags.AlwaysAutoResize);
        if (!popup) return;

        var configuration = plugin.Configuration;

        using (UiFonts.FontSize110()) ImGui.Text("Quick settings");

        ImGui.Dummy(new(0, UiSize.Em(0.25f)));
        EnumCombo("Unlock status", configuration.UnlockStatusFilter.DisplayName(),
                  state, configuration.UnlockStatusFilter,
                  FilterEnumsExtensions.DisplayName, (it, value) => it.UnlockStatusFilter = value,
                  ConfigurationEffect.Refilter);
        ImGui.Dummy(new(0, UiSize.Em(0.25f)));
        EnumCombo("Counts towards rankings", configuration.RankedFilter.DisplayName(),
                  state, configuration.RankedFilter,
                  FilterEnumsExtensions.DisplayName, (it, value) => it.RankedFilter = value,
                  ConfigurationEffect.Refilter);
        ImGui.Dummy(new(0, UiSize.Em(0.25f)));
        EnumCombo("Sort by", configuration.SortBy.DisplayName(),
                  state, configuration.SortBy,
                  FilterEnumsExtensions.DisplayName, (it, value) => it.SortBy = value,
                  ConfigurationEffect.RebuildView);
        ImGui.Dummy(new(0, UiSize.Em(0.5f)));
        if (ImGui.Button("All settings")) {
            ImGui.CloseCurrentPopup();
            state.Navigation.Navigate(new NavigationTarget.Settings());
        }
    }
}
