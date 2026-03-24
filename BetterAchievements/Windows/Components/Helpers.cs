using System.Numerics;
using Dalamud.Interface.Style;

namespace BetterAchievements.Windows.Components;

public static partial class ImGuiComponents
{
    private static readonly Vector4 DefaultGreen = new(0.1f, 0.8f, 0.2f, 1);
    private static readonly Vector4 DefaultRed = new(0.8f, 0.1f, 0.2f, 1);
    private static readonly Vector4 DefaultProgress = new(0.7f, 0.75f, 0.1f, 1f);
    private static readonly Vector4 DefaultOrange = new(1, 0.8f, 0, 1);
    private static readonly Vector4 DefaultGrey = new(0.6f, 0.6f, 0.7f, 1);
    private static readonly Vector4 DefaultBlack = new(0.0f, 0.0f, 0.0f, 1);

    private static Vector4 ColorGreen() => StyleModel.GetFromCurrent().BuiltInColors?.ParsedGreen ?? DefaultGreen;
    private static Vector4 ColorRed() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudRed ?? DefaultRed;
    private static Vector4 ColorOrange() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudOrange ?? DefaultOrange;
    private static Vector4 ColorYellow() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudYellow ?? DefaultGrey;
    private static Vector4 ColorGrey() => StyleModel.GetFromCurrent().BuiltInColors?.DalamudGrey ?? DefaultGrey;
    private static Vector4 ColorBlack() => DefaultBlack; // dalamud doesn't have a black so i guess i'm making my own

    private static Vector4 ColorProgress(float alpha = 1.0f)
    {
        return StyleModel.GetFromCurrent().BuiltInColors?.DalamudViolet ?? DefaultProgress with { W = alpha };
    }

    private static string ToRoman(int number)
    {
        if (number >= 10) return "X" + ToRoman(number - 10);
        if (number >= 9) return "IX" + ToRoman(number - 9);
        if (number >= 5) return "V" + ToRoman(number - 5);
        if (number >= 4) return "IV" + ToRoman(number - 4);
        if (number >= 1) return "I" + ToRoman(number - 1);
        return "";
    }

    private static float SizeEm(float em)
    {
        return Plugin.PluginInterface.UiBuilder.FontDefaultSizePx * em;
    }
}
