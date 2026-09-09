using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BetterAchievements.Data.Unlockable;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents {
    private const string Unknown = "Unknown";

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

    private static void IndentedWrappedText(string text, float startX, float startY, float indentX, float indentWidth, float fullWidth) {
        if (text.Length == 0) return;

        var lineHeight = ImGui.GetTextLineHeightWithSpacing();
        var lines = WrapLines(text, indentWidth, fullWidth);

        for (var i = 0; i < lines.Count; i++) {
            if (lines[i].Length == 0) continue;

            ImGui.SetCursorPos(new Vector2(i == 0 ? indentX : startX, startY + (i * lineHeight)));
            ImGui.TextUnformatted(lines[i]);
        }
    }

    private static string CollectionItemHowTo(IUnlockable unlockable) {
        var howTo = unlockable.HowTo();
        return howTo.Length > 0 ? howTo : Unknown;
    }

    public static void CollectionItem(IUnlockable unlockable) {
        using var group = ImRaii.Group();

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
        IndentedWrappedText(CollectionItemHowTo(unlockable), start.X, start.Y + lineHeight + 4, textX, availableWidth - (textX - start.X), availableWidth);

        if (unlockable.Description().Length > 0) {
            ImGui.TextColoredWrapped(UiColors.Grey(), unlockable.Description());
        }
        ImGui.Dummy(Vector2.Zero);
    }
}
