using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using BetterAchievements.Data;
using BetterAchievements.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents {
    private static void CollectionItemTitle(string name, float x, float y, float lineHeight) {
        using (UiFonts.FontSize110().Push()) {
            ImGui.SetCursorPos(new Vector2(x, y + ((lineHeight - ImGui.GetTextLineHeight()) / 2f)));
            ImGui.TextColored(UiColors.Progress(), name);
        }
    }

    private static void CollectionItemStatus(bool unlocked, float rightEdge, float y, float lineHeight) {
        var text = unlocked ? "Unlocked" : "Locked";
        var size = ImGui.CalcTextSize(text);

        ImGui.SetCursorPos(new Vector2(rightEdge - size.X, y + ((lineHeight - size.Y) / 2f)));
        ImGui.TextColored(unlocked ? UiColors.Green() : UiColors.Red(), text);
    }

    private static List<string> WrapLines(string text, float indentWidth, float fullWidth) {
        var lines = new List<string>();
        var spaceWidth = ImGui.CalcTextSize(" ").X;
        var current = new StringBuilder();
        var currentWidth = 0f;

        foreach (var paragraph in text.Replace("\r\n", "\n").Split('\n')) {
            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
                var wordWidth = ImGui.CalcTextSize(word).X;
                var maxWidth = lines.Count == 0 ? indentWidth : fullWidth;

                if (current.Length > 0 && currentWidth + spaceWidth + wordWidth > maxWidth) {
                    lines.Add(current.ToString());
                    current.Clear();
                    currentWidth = 0f;
                }

                if (current.Length > 0) {
                    current.Append(' ');
                    currentWidth += spaceWidth;
                }

                current.Append(word);
                currentWidth += wordWidth;
            }

            lines.Add(current.ToString());
            current.Clear();
            currentWidth = 0f;
        }

        return lines;
    }

    private static float IndentedWrappedText(string text, float startX, float startY, float indentX, float indentWidth, float fullWidth) {
        if (text.Length == 0) return startY;

        var lineHeight = ImGui.GetTextLineHeightWithSpacing();
        var lines = WrapLines(text, indentWidth, fullWidth);

        for (var i = 0; i < lines.Count; i++) {
            if (lines[i].Length == 0) continue;

            ImGui.SetCursorPos(new Vector2(i == 0 ? indentX : startX, startY + (i * lineHeight)));
            ImGui.TextUnformatted(lines[i]);
        }

        return startY + (lines.Count * lineHeight);
    }

    private static string CollectionItemText(CollectionEntry entry) => CompiledRegexes.HtmlTagStrip().Replace(
        string.Join("\n", new[] { entry.Item.HowTo, entry.Unlockable.Description() }.Where(it => it.Length > 0)), "");

    public static void CollectionItem(CollectionEntry entry) {
        using var group = ImRaii.Group();

        var unlockable = entry.Unlockable;
        var start = ImGui.GetCursorPos();
        var availableWidth = ImGui.GetContentRegionAvail().X;
        var iconSize = AchievementIconSize();
        var lineHeight = iconSize / 2f;
        var hasIcon = unlockable.Icon() != 0;
        var textX = hasIcon ? start.X + iconSize + ImGui.GetStyle().ItemSpacing.X : start.X;

        if (hasIcon) {
            AchievementIcon(unlockable.Icon(), iconSize);
        }

        CollectionItemTitle(unlockable.Name(), textX, start.Y, lineHeight);
        CollectionItemStatus(unlockable.Unlocked(), start.X + availableWidth, start.Y, lineHeight);

        var textBottom = IndentedWrappedText(
            CollectionItemText(entry), start.X, start.Y + lineHeight + 4, textX, availableWidth - (textX - start.X), availableWidth);
        var headerBottom = start.Y + (hasIcon ? iconSize : lineHeight);

        ImGui.SetCursorPos(start with { Y = Math.Max(headerBottom, textBottom) });
        ImGui.Dummy(Vector2.Zero);
    }
}
