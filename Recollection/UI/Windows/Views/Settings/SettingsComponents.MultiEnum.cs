using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace Recollection.UI.Windows.Views.Settings;

public static partial class SettingsComponents {
    public static void MultiEnumInput<T>(MultiEnumSetting<T> setting, Configuration draft) where T : struct, Enum {
        Label(setting);
        Description(setting);

        var selected = setting.Getter(draft);
        for (var i = 0; i < EnumSetting<T>.Values.Length; i++) {
            var value = EnumSetting<T>.Values[i];
            using var id = ImRaii.PushId(i);

            if (i > 0) ImGui.Dummy(Vector2.Zero);
            var isSelected = selected.Contains(value);
            using (PushInvalidColors(!setting.IsValid)) {
                if (ImGui.Checkbox(setting.DisplayName(value), ref isSelected)) {
                    setting.Toggle(draft, value);
                }
            }

            EnumValueDescription(setting.ValueDescription?.Invoke(value));
        }
    }
}

public record MultiEnumSetting<T> : Setting<IReadOnlySet<T>> where T : struct, Enum {
    public required Func<T, string> DisplayName { get; init; }
    public Func<T, string?>? ValueDescription { get; init; }
    public Func<IReadOnlySet<T>, bool>? Validator { get; init; }

    public override void Load(Configuration draft) => Validate(Getter(draft));

    public override void Draw(Configuration draft) => SettingsComponents.MultiEnumInput(this, draft);

    public void Toggle(Configuration draft, T value) {
        var selected = new HashSet<T>(Getter(draft));
        if (!selected.Remove(value)) selected.Add(value);

        Validate(selected);
        Update(draft, selected);
    }

    private void Validate(IReadOnlySet<T> selected) {
        IsValid = Validator?.Invoke(selected) ?? true;
    }
}
