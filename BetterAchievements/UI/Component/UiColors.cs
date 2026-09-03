using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Style;

namespace BetterAchievements.UI.Component;

public static class UiColors {
    private static readonly Dictionary<string, Func<Vector4>> NamedColors = new(StringComparer.OrdinalIgnoreCase) {
        ["Green"] = Green,
        ["Red"] = Red,
        ["Orange"] = Orange,
        ["Yellow"] = Yellow,
        ["Grey"] = Grey,
        ["Black"] = Black,
        ["Progress"] = Progress,
        ["Violet"] = Violet,
        ["Text"] = Text,
        ["Blue"] = Blue,
    };

    private static readonly Vector4 DefaultGreen = new(0.1f, 0.8f, 0.2f, 1);

    private static readonly Vector4 DefaultRed = new(0.8f, 0.1f, 0.2f, 1);

    // old color
    // private static readonly Vector4 DefaultProgress = new(0.9f, 0.75f, 0.5f, 1f);
    private static readonly Vector4 DefaultProgress = new(0.975f, 0.85f, 0.3f, 1f);
    private static readonly Vector4 DefaultOrange = new(1f, 0.8f, 0f, 1f);
    private static readonly Vector4 DefaultGrey = new(0.6f, 0.6f, 0.7f, 1f);
    private static readonly Vector4 DefaultBlack = new(0f, 0f, 0f, 1f);
    private static readonly Vector4 DefaultViolet = new(0.65f, 0.5f, 0.95f, 1f);
    private static readonly Vector4 DefaultBlue = new(0.3f, 0.6f, 1f, 1f);

    public static Vector4 Green() => StyleModel.GetFromCurrent().BuiltInColors?.ParsedGreen ?? DefaultGreen;
    public static Vector4 Red() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudRed ?? DefaultRed;
    public static Vector4 Orange() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudOrange ?? DefaultOrange;
    public static Vector4 Yellow() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudYellow ?? DefaultGrey;
    public static Vector4 Grey() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudGrey ?? DefaultGrey;
    public static Vector4 Black() => DefaultBlack; // dalamud doesn't have a black so i guess i'm making my own
    public static Vector4 Progress() => DefaultProgress;
    public static Vector4 Violet() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudViolet ?? DefaultViolet;
    public static Vector4 Blue() => StyleModel.GetFromCurrent().BuiltInColors?.ParsedBlue ?? DefaultBlue;
    public static Vector4 Text() => UIntColorToVector(ImGui.GetColorU32(ImGuiCol.Text));
    public static Vector4 WindowBackground() => UIntColorToVector(ImGui.GetColorU32(ImGuiCol.WindowBg));

    public static Vector4? Parse(string? value) {
        if (value == null) return null;
        return NamedColors.TryGetValue(value, out var color) ? color() : null;
    }

    public static Vector4 Brightness(this Vector4 v, float brightness) {
        brightness = Math.Clamp(brightness, -1.0f, 1.0f);
        if (brightness >= 0.0f) {
            return new Vector4 {
                X = ((1.0f - v.X) * brightness) + v.X,
                Y = ((1.0f - v.Y) * brightness) + v.Y,
                Z = ((1.0f - v.Z) * brightness) + v.Z,
                W = v.W
            };
        }

        var factor = 1 + brightness;
        return new Vector4 {
            X = v.X * factor,
            Y = v.Y * factor,
            Z = v.Z * factor,
            W = v.W
        };
    }

    private static Vector4 UIntColorToVector(uint color) {
        return new Vector4 {
            X = (color & 0xFF) / 255.0f,
            Y = ((color >> 8) & 0xFF) / 255.0f,
            Z = ((color >> 16) & 0xFF) / 255.0f,
            W = ((color >> 24) & 0xFF) / 255.0f
        };
    }
}
