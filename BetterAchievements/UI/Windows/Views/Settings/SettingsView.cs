using System.Linq;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Windows.Views.Settings;

public class SettingsView : IView {
    private const float SaveButtonHeightEm = 2.5f;

    private readonly Plugin plugin;
    private readonly UnlockablesState unlockables;
    private readonly Configuration draft;
    private readonly SettingsSection[] sections = [
        new("Filters", [
            new EnumSetting<UnlockStatusFilter> {
                Name = "Filter by unlock status",
                ValueName = FilterEnumsExtensions.DisplayName,
                ValueDescription = FilterEnumsExtensions.DisplayDescription,
                Getter = configuration => configuration.UnlockStatusFilter,
                Setter = (configuration, value) => configuration.UnlockStatusFilter = value,
            },
            new EnumSetting<ContainsRewardsFilter> {
                Name = "Filter by reward type",
                Description = "Feature coming soon!",
                IsDisabled = _ => true,
                ValueName = FilterEnumsExtensions.DisplayName,
                Getter = configuration => configuration.ContainsRewardsFilter,
                Setter = (configuration, value) => configuration.ContainsRewardsFilter = value,
            },
            new EnumSetting<RankedFilter> {
                Name = "Filter items counting towards leaderboards",
                Description = "Currently only applies to achievements",
                ValueName = FilterEnumsExtensions.DisplayName,
                ValueDescription = FilterEnumsExtensions.DisplayDescription,
                Getter = configuration => configuration.RankedFilter,
                Setter = (configuration, value) => configuration.RankedFilter = value,
            },
            new EnumSetting<AreaFilter> {
                Name = "Filter by area",
                Description = "Feature coming soon!",
                IsDisabled = _ => true,
                ValueName = FilterEnumsExtensions.DisplayName,
                Getter = configuration => configuration.AreaFilter,
                Setter = (configuration, value) => configuration.AreaFilter = value,
            },
        ]),
        new("Sorting options", [
            new EnumSetting<SortBy> {
                Name = "Sorting order",
                ValueName = FilterEnumsExtensions.DisplayName,
                Getter = configuration => configuration.SortBy,
                Setter = (configuration, value) => configuration.SortBy = value,
            },
        ]),
        new("Other settings", [
            new BooleanSetting {
                Name = "Never hide progress bars",
                Getter = configuration => configuration.NeverHideProgressBars,
                Setter = (configuration, value) => configuration.NeverHideProgressBars = value,
            },
            new BooleanSetting {
                Name = "Display IDs",
                Description = "Only useful for development!",
                Getter = configuration => configuration.DisplayIds,
                Setter = (configuration, value) => configuration.DisplayIds = value,
            },
            new BooleanSetting {
                Name = "Enable debug mode",
                Description = "Only useful for development!",
                Getter = configuration => configuration.DebugMode,
                Setter = (configuration, value) => configuration.DebugMode = value,
            },
        ]),
    ];

    public SettingsView(Plugin plugin, UnlockablesState unlockables) {
        this.plugin = plugin;
        this.unlockables = unlockables;
        draft = plugin.Configuration.Clone();

        foreach (var section in sections) {
            foreach (var setting in section.Settings) {
                setting.Load(draft);
            }
        }
    }

    private bool HasValidationErrors() {
        foreach (var section in sections) {
            foreach (var setting in section.Settings) {
                if (!setting.IsValid) return true;
            }
        }

        return false;
    }

    private void Save() {
        foreach (var section in sections) {
            foreach (var setting in section.Settings) {
                setting.Apply(draft, plugin.Configuration);
                setting.IsChanged = false;
            }
        }

        plugin.Configuration.Save();
        unlockables.ApplyFilters();
    }

    public void Draw() {
        var ySize = UiSize.MainContentHeight(plugin.Configuration);
        using var mainContent = ImRaii.Child("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true, ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!mainContent) return;

        var saveButtonHeight = UiSize.Em(SaveButtonHeightEm);
        var sectionsHeight = ImGui.GetContentRegionAvail().Y - saveButtonHeight - ImGui.GetStyle().ItemSpacing.Y;

        using (var sectionsChild = ImRaii.Child("SettingsSections", ImGui.GetContentRegionAvail() with { Y = sectionsHeight })) {
            if (sectionsChild) {
                ImGui.Dummy(new(0, UiSize.Em(0.5f)));

                foreach (var section in sections) {
                    SettingsComponents.Section(section, draft);
                }
            }
        }

        if (SettingsComponents.SaveButton(HasValidationErrors(), saveButtonHeight)) {
            Save();
        }
    }
}
