using System;
using System.Globalization;
using System.Numerics;
using BetterAchievements.UI.Component;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Windows.Views.Settings;

public static partial class SettingsComponents {
    private static ImRaii.ColorDisposable PushInvalidColors(bool invalid) {
        var red = UiColors.Red();
        return ImRaii.PushColor(ImGuiCol.FrameBg, red with { W = 0.35f }, invalid)
                     .Push(ImGuiCol.FrameBgHovered, red with { W = 0.45f }, invalid)
                     .Push(ImGuiCol.FrameBgActive, red with { W = 0.55f }, invalid);
    }

    private static bool ValidatedTextInput(ISetting setting, ref string buffer) {
        Label(setting);
        Description(setting);

        using var invalidColors = PushInvalidColors(!setting.IsValid);
        return ImGui.InputText("##Input", ref buffer);
    }

    public static void TextInput(TextSetting setting, Configuration draft) {
        if (ValidatedTextInput(setting, ref setting.Buffer)) {
            setting.Input(draft);
        }
    }

    public static void IntegerInput<T>(IntegerSetting<T> setting, Configuration draft) where T : struct, INumberBase<T> {
        if (ValidatedTextInput(setting, ref setting.Buffer)) {
            setting.Input(draft);
        }
    }

    public static void FloatInput<T>(FloatSetting<T> setting, Configuration draft) where T : struct, INumberBase<T> {
        if (ValidatedTextInput(setting, ref setting.Buffer)) {
            setting.Input(draft);
        }
    }
}

public record IntegerSetting<T> : Setting<T> where T : struct, INumberBase<T> {
    public Func<T, bool>? Validator { get; init; }

    public string Buffer = "";

    public override void Load(Configuration draft) {
        var value = Getter(draft);
        Buffer = value.ToString()!;
        IsValid = Validator?.Invoke(value) ?? true;
    }

    public override void Draw(Configuration draft) => SettingsComponents.IntegerInput(this, draft);

    public void Input(Configuration draft) {
        IsValid = T.TryParse(Buffer, NumberStyles.Integer, null, out var value) && (Validator?.Invoke(value) ?? true);
        if (IsValid) Update(draft, value);
    }
}

public record FloatSetting<T> : Setting<T> where T : struct, INumberBase<T>  {
    public Func<T, bool>? Validator { get; init; }

    public string Buffer = "";

    public override void Load(Configuration draft) {
        var value = Getter(draft);
        Buffer = value.ToString()!;
        IsValid = Validator?.Invoke(value) ?? true;
    }

    public override void Draw(Configuration draft) => SettingsComponents.FloatInput(this, draft);

    public void Input(Configuration draft) {
        IsValid = T.TryParse(Buffer, NumberStyles.Float, null, out var value) && (Validator?.Invoke(value) ?? true);
        if (IsValid) Update(draft, value);
    }
}

public record TextSetting : Setting<string> {
    public Func<string, bool>? Validator { get; init; }

    public string Buffer = "";

    public override void Load(Configuration draft) {
        Buffer = Getter(draft);
        IsValid = Validator?.Invoke(Buffer) ?? true;
    }

    public override void Draw(Configuration draft) => SettingsComponents.TextInput(this, draft);

    public void Input(Configuration draft) {
        IsValid = Validator?.Invoke(Buffer) ?? true;
        if (IsValid) Update(draft, Buffer);
    }
}
