using System;
using System.Numerics;
using Dalamud.Interface.Style;

namespace BetterAchievements.UI.Component;

public static class UiColors
{
    private static readonly Vector4 DefaultGreen = new(0.1f, 0.8f, 0.2f, 1);
    private static readonly Vector4 DefaultRed = new(0.8f, 0.1f, 0.2f, 1);
    // private static readonly Vector4 DefaultProgress = new(0.7f, 0.75f, 0.1f, 1f);
    private static readonly Vector4 DefaultProgress = new(0.9f, 0.75f, 0.5f, 1f);
    private static readonly Vector4 DefaultOrange = new(1, 0.8f, 0, 1);
    private static readonly Vector4 DefaultGrey = new(0.6f, 0.6f, 0.7f, 1);
    private static readonly Vector4 DefaultBlack = new(0.0f, 0.0f, 0.0f, 1);

    public static Vector4 ColorGreen() => StyleModel.GetFromCurrent().BuiltInColors?.ParsedGreen ?? DefaultGreen;
    public static Vector4 ColorRed() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudRed ?? DefaultRed;
    public static Vector4 ColorOrange() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudOrange ?? DefaultOrange;
    public static Vector4 ColorYellow() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudYellow ?? DefaultGrey;
    public static Vector4 ColorGrey() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudGrey ?? DefaultGrey;
    public static Vector4 ColorBlack() => DefaultBlack; // dalamud doesn't have a black so i guess i'm making my own
    public static Vector4 ColorProgress() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudOrange ?? DefaultProgress;

    public static Vector4 Brightness(this Vector4 v, float brightness)
    {
        brightness = Math.Clamp(brightness, -1.0f, 1.0f);
        if (brightness >= 0.0f)
        {
            return new()
            {
                X = (1.0f - v.X) * brightness + v.X,
                Y = (1.0f - v.Y) * brightness + v.Y,
                Z = (1.0f - v.Z) * brightness + v.Z,
                W = v.W
            };
        }
        var factor = 1 + brightness;
        return new()
        {
            X = v.X * factor,
            Y = v.Y * factor,
            Z = v.Z * factor,
            W = v.W
        };
    }
}
