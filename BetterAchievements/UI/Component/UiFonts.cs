using System.Collections.Generic;
using Dalamud.Interface.ManagedFontAtlas;

namespace BetterAchievements.UI.Component;

public static class UiFonts {
    private static readonly int[] Percentages = [60, 70, 80, 90, 100, 110, 125, 150, 175, 200, 250, 300, 400];

    private static readonly Dictionary<int, IFontHandle> Fonts = new();

    public static void Initialize() {
        foreach (var percentage in Percentages) {
            Fonts[percentage] = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(e =>
                e.OnPreBuild(tk => tk.AddDalamudDefaultFont(Plugin.PluginInterface.UiBuilder.FontDefaultSizePx * percentage / 100f)));
        }
    }

    public static void Dispose() {
        foreach (var font in Fonts.Values) {
            font.Dispose();
        }

        Fonts.Clear();
    }

    public static IFontHandle FontSize60() => Fonts[60];
    public static IFontHandle FontSize70() => Fonts[70];
    public static IFontHandle FontSize80() => Fonts[80];
    public static IFontHandle FontSize90() => Fonts[90];
    public static IFontHandle FontSize100() => Fonts[100];
    public static IFontHandle FontSize110() => Fonts[110];
    public static IFontHandle FontSize125() => Fonts[125];
    public static IFontHandle FontSize150() => Fonts[150];
    public static IFontHandle FontSize175() => Fonts[175];
    public static IFontHandle FontSize200() => Fonts[200];
    public static IFontHandle FontSize250() => Fonts[250];
    public static IFontHandle FontSize300() => Fonts[300];
    public static IFontHandle FontSize400() => Fonts[400];
}
