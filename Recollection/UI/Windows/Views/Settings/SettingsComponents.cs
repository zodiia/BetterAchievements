using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.UI.Component;

namespace Recollection.UI.Windows.Views.Settings;

public static partial class SettingsComponents {
    private static void SettingRow(Plugin plugin, ISetting setting, Configuration draft) {
        using var id = ImRaii.PushId(setting.Name);
        using var disabled = ImRaii.Disabled(setting.IsDisabled?.Invoke(draft) ?? false);
        using var itemWidth = ImRaii.ItemWidth(UiSize.Em(16));

        setting.Draw(draft);
        ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));
    }

    private static void Label(ISetting setting) {
        ImGui.TextWrapped(setting.Name);
    }

    private static void Description(ISetting setting) {
        if (setting.Description == null) return;

        using var descriptionColor = ImRaii.PushColor(ImGuiCol.Text, UiColors.Grey());
        ImGui.TextWrapped(setting.Description);
    }

    public static void Section(Plugin plugin, SettingsSection section, Configuration draft) {
        using var id = ImRaii.PushId(section.Name);

        if (UiComponents.SectionHeader(section.Name, plugin.Configuration.UiDensity, paddingAboveEm: 0.5f, open: section.IsOpen)) {
            section.IsOpen = !section.IsOpen;
        }

        if (!section.IsOpen) return;

        foreach (var setting in section.Settings) {
            SettingRow(plugin, setting, draft);
            ImGui.Dummy(new(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));
        }
    }

    public static bool SaveButton(bool disabled, float height) {
        using var disabledScope = ImRaii.Disabled(disabled);
        using (UiFonts.FontSize125().Push()) {
            return ImGui.Button("Save settings", new Vector2(ImGui.GetContentRegionAvail().X, height));
        }
    }
}
