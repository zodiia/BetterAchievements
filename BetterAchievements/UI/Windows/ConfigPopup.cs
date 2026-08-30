using System;
using BetterAchievements.Data;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Windows;

public static class ConfigPopup {
    public const string FiltersPopupId = "FilterPopup";

    private static void EnumCombo<T>(string label, string preview, MainWindowState state, T current, Func<T, string> displayName,
                                     Action<Configuration, T> apply, ConfigurationEffect effect) where T : struct, Enum {
        using var combo = ImRaii.Combo(label, preview, ImGuiComboFlags.HeightLargest);
        if (!combo) return;

        foreach (var value in Enum.GetValues<T>()) {
            if (ImGui.Selectable(displayName(value), current.Equals(value))) {
                state.UpdateConfiguration(configuration => apply(configuration, value), effect);
            }
        }
    }

    private static void Checkbox(string label, MainWindowState state, bool current, Action<Configuration, bool> apply) {
        var value = current;
        if (ImGui.Checkbox(label, ref value)) {
            state.UpdateConfiguration(configuration => apply(configuration, value));
        }
    }

    public static void FiltersPopup(Plugin plugin, MainWindowState state) {
        using var popup = ImRaii.Popup(FiltersPopupId, ImGuiWindowFlags.AlwaysAutoResize);
        if (!popup) return;

        var configuration = plugin.Configuration;

        ImGui.Text("Filters");

        EnumCombo("Unlock status", configuration.UnlockStatusFilter.DisplayName(),
                  state, configuration.UnlockStatusFilter,
                  FilterEnumsExtensions.DisplayName, (it, value) => it.UnlockStatusFilter = value,
                  ConfigurationEffect.Refilter);

        EnumCombo("Contains rewards (title, minion, ...)", "(not implemented)",
                  state, configuration.ContainsRewardsFilter,
                  FilterEnumsExtensions.DisplayName, (it, value) => it.ContainsRewardsFilter = value,
                  ConfigurationEffect.Refilter);

        EnumCombo("Counts towards rankings", configuration.RankedFilter.DisplayName(),
                  state, configuration.RankedFilter,
                  FilterEnumsExtensions.DisplayName, (it, value) => it.RankedFilter = value,
                  ConfigurationEffect.Refilter);

        EnumCombo("Area", "(not implemented)",
                  state, configuration.AreaFilter,
                  FilterEnumsExtensions.DisplayName, (it, value) => it.AreaFilter = value,
                  ConfigurationEffect.Refilter);

        ImGui.Text("Sorting options");

        EnumCombo("Sort by", configuration.SortBy.DisplayName(),
                  state, configuration.SortBy,
                  FilterEnumsExtensions.DisplayName, (it, value) => it.SortBy = value,
                  ConfigurationEffect.RebuildView);

        ImGui.Text("Other settings");

        Checkbox("Display IDs", state,
                 configuration.DisplayIds, (it, value) => it.DisplayIds = value);
        Checkbox("Never hide progress bars", state,
                 configuration.NeverHideProgressBars, (it, value) => it.NeverHideProgressBars = value);
        Checkbox("Enable debug mode", state,
                 configuration.DebugMode, (it, value) => it.DebugMode = value);
    }
}
