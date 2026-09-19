using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.Data.Unlockable;

namespace Recollection.UI.Component;

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

    public static void CollectionItem(IUnlockable unlockable, Configuration config) {
        using var group = ImRaii.Group();

        var start = ImGui.GetCursorPos();
        var availableWidth = ImGui.GetContentRegionAvail().X;
        var iconSize = AchievementIconSize();
        var lineHeight = iconSize / 2f;
        var hasIcon = unlockable.Icon() != 0;
        var textX = hasIcon ? start.X + iconSize + ImGui.GetStyle().ItemSpacing.X : start.X;
        var howTo = unlockable.HowTo();
        var howToY = start.Y + lineHeight + 4;
        var howToHeight = ImGui.GetTextLineHeight();

        if (hasIcon) {
            AchievementIcon(unlockable.Icon(), iconSize);
        }

        CollectionItemTitle(unlockable.Name(), textX, start.Y, howTo?.Length is > 0 ? lineHeight : howToY - start.Y + howToHeight);
        if (config.DisplayIds) {
            ImGui.SameLine();
            ImGui.TextDisabled($"#{unlockable.Id()}");
        }

        CollectionItemStatus(unlockable.Unlocked(), start.X + availableWidth, start.Y, lineHeight);
        if (howTo?.Length is > 0) {
            IndentedWrappedText(howTo, start.X, howToY, textX, availableWidth - (textX - start.X), availableWidth);
        } else {
            ImGui.SetCursorPos(start with { Y = howToY });
            ImGui.Dummy(new Vector2(0f, howToHeight));
        }

        if (unlockable.Description().Length > 0) {
            ImGui.TextColoredWrapped(UiColors.Grey(), unlockable.Description());
        }

        if (unlockable.Maximum() > 1 && (!unlockable.Unlocked() || config.NeverHideProgressBars)) {
            var progress = unlockable.Current();

            ProgressBar(
                (progress ?? 1.0f) / unlockable.Maximum(),
                progress != null ? UiColors.Progress() : UiColors.Red(),
                height: UiSize.Em(config.ProgressBarHeight),
                insideText: progress != null ? $"{progress}/{unlockable.Maximum()}" : "Not loaded",
                enabled: progress != null);
        }


        ImGui.Dummy(Vector2.Zero);
    }
}
