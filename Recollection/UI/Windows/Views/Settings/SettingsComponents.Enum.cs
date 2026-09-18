using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.UI.Component;

namespace Recollection.UI.Windows.Views.Settings;

public static partial class SettingsComponents {
    private static void EnumValueDescription(string? description) {
        if (description == null) return;

        using var indent = ImRaii.PushIndent(ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X, false);
        using var descriptionColor = ImRaii.PushColor(ImGuiCol.Text, UiColors.Grey());
        ImGui.TextWrapped(description);
    }

    public static void EnumInput<T>(EnumSetting<T> setting, Configuration draft) where T : struct, Enum {
        Label(setting);
        Description(setting);

        var current = setting.Getter(draft);
        for (var i = 0; i < EnumSetting<T>.Values.Length; i++) {
            var value = EnumSetting<T>.Values[i];
            using var id = ImRaii.PushId(i);

            if (i > 0) ImGui.Dummy(Vector2.Zero);
            if (ImGui.RadioButton(setting.ValueName(value), EqualityComparer<T>.Default.Equals(current, value))) {
                setting.Update(draft, value);
            }

            EnumValueDescription(setting.ValueDescription?.Invoke(value));
        }
    }
}

public record EnumSetting<T> : Setting<T> where T : struct, Enum {
    public static readonly T[] Values = Enum.GetValues<T>();

    public required Func<T, string> ValueName { get; init; }
    public Func<T, string?>? ValueDescription { get; init; }

    public override void Draw(Configuration draft) => SettingsComponents.EnumInput(this, draft);
}
