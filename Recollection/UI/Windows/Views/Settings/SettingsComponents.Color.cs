using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;

namespace Recollection.UI.Windows.Views.Settings;

public static partial class SettingsComponents {
    public static void ColorInput(ColorSetting setting, Configuration draft) {
        var current = setting.Getter(draft);
        var color = ImGuiComponents.ColorPickerWithPalette((int)ImGui.GetID("##Input"), setting.Name, current);
        if (color != current) {
            setting.Update(draft, color);
        }

        ImGui.SameLine();
        Label(setting);
        Description(setting);
    }
}

public record ColorSetting : Setting<Vector4> {
    public override void Draw(Configuration draft) => SettingsComponents.ColorInput(this, draft);
}
