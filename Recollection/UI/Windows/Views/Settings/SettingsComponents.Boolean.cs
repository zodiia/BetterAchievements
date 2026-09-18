using Dalamud.Bindings.ImGui;

namespace Recollection.UI.Windows.Views.Settings;

public static partial class SettingsComponents {
    public static void BooleanInput(BooleanSetting setting, Configuration draft) {
        var value = setting.Getter(draft);
        if (ImGui.Checkbox("##Input", ref value)) {
            setting.Update(draft, value);
        }

        ImGui.SameLine();
        Label(setting);
        Description(setting);
    }
}

public record BooleanSetting : Setting<bool> {
    public override void Draw(Configuration draft) => SettingsComponents.BooleanInput(this, draft);
}
