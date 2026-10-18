using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Recollection.UI.Component;
using AchievementRoulette = Recollection.UI.State.AchievementRoulette;

namespace Recollection.UI.Windows.Views.Roulette;

public static partial class RouletteComponents {
    private const string DayFormat = "dddd, MMMM d";

    private static void Graph(AchievementRoulette roulette, Vector4 color) {
        var drawList = ImGui.GetWindowDrawList();
        var barWidth = UiSize.Em(0.5f);
        var barGap = UiSize.Em(0.15f);
        var height = ImGui.GetTextLineHeight();
        var start = ImGui.GetCursorScreenPos();
        var highest = Math.Max(1u, roulette.Daily.DefaultIfEmpty(0u).Max());
        var mouse = ImGui.GetMousePos();

        for (var i = 0; i < roulette.Daily.Length; i++) {
            var value = roulette.Daily[i];
            var barHeight = value == 0 ? UiSize.Em(0.1f) : Math.Max(UiSize.Em(0.2f), height * value / highest);
            var min = new Vector2(start.X + (i * (barWidth + barGap)), start.Y + height - barHeight);
            var max = new Vector2(min.X + barWidth, start.Y + height);
            var day = roulette.RolledAt.Date.AddDays(i);
            drawList.AddRectFilled(min, max, ImGui.GetColorU32(value == 0 ? UiColors.Grey() with { W = 0.5f } : color), 1f);

            if (mouse.X >= min.X && mouse.X < max.X + barGap && mouse.Y >= start.Y && mouse.Y <= start.Y + height) {
                ImGui.SetTooltip($"Day {i + 1}, {day.ToString(DayFormat, CultureInfo.InvariantCulture)}\n+{value:N0}");
            }
        }

        ImGui.Dummy(new Vector2((roulette.Daily.Length * (barWidth + barGap)) - barGap, height));
    }

    private static void KeyValue(string key, string value, Vector4 color, string? suffix = null) {
        ImGui.TableNextColumn();
        ImGui.TextColored(UiColors.Grey(), key);
        ImGui.TableNextColumn();
        ImGui.TextColored(color, value);
        if (suffix == null) return;

        ImGui.SameLine();
        ImGui.TextColored(UiColors.Grey(), suffix);
    }

    private static void RouletteHeader(AchievementRoulette achievementRoulette) {
        var achievement = achievementRoulette.Achievement;
        var iconSize = ImGui.GetTextLineHeightWithSpacing() * 3;
        var wrap = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(achievement.Icon)).GetWrapOrEmpty();

        ImGui.Image(wrap.Handle, new Vector2(iconSize, iconSize));
        ImGui.SameLine();

        ImGui.BeginGroup();
        using (UiFonts.FontSize110().Push()) ImGui.TextColored(UiColors.Progress(), achievement.Name.ToString());
        ImGui.TextColored(UiColors.Progress(), $"{achievement.Points} points");
        ImGui.TextColored(UiColors.Grey(), achievementRoulette.Breadcrumb);
        ImGui.EndGroup();

        ImGui.TextWrapped(achievement.Description.ToString());
    }
}
