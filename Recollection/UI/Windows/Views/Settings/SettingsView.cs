using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.Data;
using Recollection.UI.Component;
using Recollection.UI.State;

namespace Recollection.UI.Windows.Views.Settings;

public class SettingsView : IView {
    private const float SaveButtonHeightEm = 2.5f;

    private readonly Plugin plugin;
    private readonly UnlockablesState unlockables;
    private readonly Configuration draft;
    private readonly SettingsSection[] sections = [
        new("UI settings", [
            new FloatSetting<float> {
                Name = "Progress bars size in collections",
                Description = "Must be between 0.5 and 5.0.",
                Validator = it => it is >= 0.5f and <= 5.0f,
                Getter = it => it.ProgressBarHeight,
                Setter = (it, value) => it.ProgressBarHeight = value,
            },
            new FloatSetting<float> {
                Name = "Progress bars size in the sidebar",
                Description = "Must be between 0.0 and 1.0. Setting it to 0 would disable them.",
                Validator = it => it is >= 0.0f and <= 1.0f,
                Getter = it => it.SidebarProgressBarHeight,
                Setter = (it, value) => it.SidebarProgressBarHeight = value,
            },
            new BooleanSetting {
                Name = "Never hide progress bars",
                Description = "By default, progress bars are hidden under certain conditions, like when the progress is 100%. Turn this on to disable that behavior.",
                Getter = it => it.NeverHideProgressBars,
                Setter = (it, value) => it.NeverHideProgressBars = value,
            },
        ]),
        new("Filters and sorting options", [
            new EnumSetting<UnlockStatusFilter> {
                Name = "Filter by unlock status",
                ValueName = ConfigEnumsExtensions.DisplayName,
                ValueDescription = ConfigEnumsExtensions.DisplayDescription,
                Getter = it => it.UnlockStatusFilter,
                Setter = (it, value) => it.UnlockStatusFilter = value,
            },
            new EnumSetting<ContainsRewardsFilter> {
                Name = "Filter by reward type",
                Description = "Feature coming soon!",
                IsDisabled = _ => true,
                ValueName = ConfigEnumsExtensions.DisplayName,
                Getter = it => it.ContainsRewardsFilter,
                Setter = (it, value) => it.ContainsRewardsFilter = value,
            },
            new EnumSetting<RankedFilter> {
                Name = "Filter items counting towards leaderboards",
                Description = "Currently only applies to achievements.",
                ValueName = ConfigEnumsExtensions.DisplayName,
                ValueDescription = ConfigEnumsExtensions.DisplayDescription,
                Getter = it => it.RankedFilter,
                Setter = (it, value) => it.RankedFilter = value,
            },
            new EnumSetting<AreaFilter> {
                Name = "Filter by area",
                Description = "Feature coming soon!",
                IsDisabled = _ => true,
                ValueName = ConfigEnumsExtensions.DisplayName,
                Getter = it => it.AreaFilter,
                Setter = (it, value) => it.AreaFilter = value,
            },
        ]),
        new("Sorting options", [
            new EnumSetting<SortBy> {
                Name = "Sorting order",
                ValueName = ConfigEnumsExtensions.DisplayName,
                Getter = it => it.SortBy,
                Setter = (it, value) => it.SortBy = value,
            },
        ]),
        new("Other settings", [
            new BooleanSetting {
                Name = "Display IDs",
                Description = "Only useful for development!",
                Getter = it => it.DisplayIds,
                Setter = (it, value) => it.DisplayIds = value,
            },
            new BooleanSetting {
                Name = "Enable debug mode",
                Description = "Only useful for development!",
                Getter = it => it.DebugMode,
                Setter = (it, value) => it.DebugMode = value,
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
