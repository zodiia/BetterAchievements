using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Recollection.Data;
using Recollection.Data.Unlockable;

namespace Recollection.UI.Component;

public static partial class UiComponents {
    private static string ToRoman(uint number) => ToRoman((int)number);

    private static string ToRoman(int number) {
        return number switch {
            >= 100 => "C" + ToRoman(number - 100),
            >= 90 => "XC" + ToRoman(number - 90),
            >= 50 => "L" + ToRoman(number - 50),
            >= 40 => "XL" + ToRoman(number - 40),
            >= 10 => "X" + ToRoman(number - 10),
            >= 9 => "IX" + ToRoman(number - 9),
            >= 5 => "V" + ToRoman(number - 5),
            >= 4 => "IV" + ToRoman(number - 4),
            >= 1 => "I" + ToRoman(number - 1),
            _ => ""
        };
    }

    private static unsafe void OpenAchievementWindow(uint achievementId) {
        if (AgentModule.Instance() is null || AgentModule.Instance()->GetAgentAchievement() is null) {
            return;
        }

        AgentModule.Instance()->GetAgentAchievement()->OpenById(achievementId);
    }

    public static void SameLineRightTextColored(Vector4 color, string text) {
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextColored(color, text);
    }

    private static void Pin(bool active, IEnumerable<uint> ids, Configuration configuration) {
        var color = active ? UiColors.Progress() : UiColors.Grey();
        var hoverText = active ? "Unpin this achievement" : "Pin this achievement";
        var icon = active ? FontAwesomeIcon.Thumbtack : FontAwesomeIcon.ThumbtackSlash;
        var boxStart = ImGui.GetCursorScreenPos();
        Vector2 boxEnd;

        using (var _ = ImRaii.PushFont(UiBuilder.IconFont)) {
            var iconText = icon.ToIconString(); // this one is bigger
            var textSize = ImGui.CalcTextSize(iconText);
            boxEnd = new Vector2(boxStart.X + textSize.X, boxStart.Y + textSize.Y);
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 1); // for some reason it clips by 1px by default
            ImGui.TextColored(color, iconText);
            ImGui.SameLine();
            if (active) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3); // necessary readjustments
        }

        if (ImGui.IsMouseHoveringRect(boxStart, boxEnd)) {
            ImGui.SetTooltip(hoverText);
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        if (ImGui.IsItemClicked()) {
            if (active) {
                configuration.PinnedAchievements.RemoveAll(ids.Contains);
            } else {
                configuration.PinnedAchievements.Add(ids.Last());
            }

            configuration.Save();
        }
    }

    private static float AchievementIconSize() => ImGui.GetTextLineHeightWithSpacing() * 2;

    private static void AchievementIcon(uint iconId, float size) {
        var wrap = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrEmpty();
        ImGui.Image(wrap.Handle, new Vector2(size, size));
    }

    private static void AchievementHeaderLine1(Action drawRightSide) {
        ImGui.Dummy(Vector2.Zero);
        drawRightSide();
    }

    private static void AchievementHeaderTitle(string name, uint? displayId, bool pinned, IEnumerable<uint> ids, Configuration configuration) {
        var restore = ImGui.GetCursorPos();
        var middle = restore.Y + (ImGui.GetTextLineHeight() * 2 + ImGui.GetStyle().ItemSpacing.Y * 2) / 2f;

        float pinHeight;
        using (ImRaii.PushFont(UiBuilder.IconFont)) pinHeight = ImGui.GetTextLineHeight();
        float titleHeight;
        using (UiFonts.FontSize110().Push()) {
            var font = ImGui.GetFont();
            titleHeight = ImGui.GetTextLineHeight() * font.Ascent / font.FontSize;
        }

        ImGui.SetCursorPosY(middle - pinHeight / 2f);
        Pin(pinned, ids, configuration);

        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX(), middle - titleHeight / 2f - 2f));
        using (UiFonts.FontSize110().Push()) ImGui.TextColored(UiColors.Progress(), name);
        if (displayId.HasValue) {
            ImGui.SameLine();
            ImGui.TextDisabled(" #" + displayId.Value);
        }

        ImGui.SetCursorPos(restore);
    }

    private static void AchievementRightHeaderTiered(UnlockableTieredAchievement achievements, Configuration config) {
        if (achievements.Maximum() >= 14) {
            TieredAchievementSimpleTiers(achievements, config);
        } else {
            TieredAchievementTiers(achievements, config);
        }
    }

    private static void AchievementHeaderLine2(string pointsText) {
        // placeholder for later features?
        ImGui.Dummy(Vector2.Zero);
        SameLineRightTextColored(UiColors.Progress(), pointsText);
    }

    private static void AchievementDescriptionSimple(UnlockableAchievement achievement, Configuration configuration) {
        ImGui.TextWrapped(achievement.Description());

        if (achievement.Maximum() <= 1 || achievement.Unlocked()) return;

        var progress = achievement.Current();

        ProgressBar(
            (progress ?? 1.0f) / achievement.Maximum(),
            progress != null ? UiColors.Progress() : UiColors.Red(),
            height: UiSize.Em(configuration.ProgressBarHeight),
            insideText: progress != null ? $"{achievement.Current()}/{achievement.Maximum()}" : "Not loaded (click to refresh)",
            tooltip: "Click to refresh",
            enabled: progress != null,
            onClick: () => OpenAchievementWindow(achievement.Id()));
    }

    private static void WrappedColoredText(params (string Text, Vector4? Color)[] segments) {
        var wrapWidth = ImGui.GetContentRegionAvail().X;
        var spaceWidth = ImGui.CalcTextSize(" ").X;
        var lineWidth = 0f;
        var first = true;

        foreach (var (text, color) in segments) {
            var lines = text.Replace("\r\n", "\n").Split('\n');
            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++) {
                if (lineIndex > 0) {
                    if (lines[lineIndex].Length == 0) ImGui.NewLine();
                    lineWidth = 0f;
                    first = true;
                }

                foreach (var word in lines[lineIndex].Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
                    var wordWidth = ImGui.CalcTextSize(word).X;

                    if (!first && lineWidth + spaceWidth + wordWidth <= wrapWidth) {
                        ImGui.SameLine(0, spaceWidth);
                        lineWidth += spaceWidth + wordWidth;
                    } else {
                        lineWidth = wordWidth;
                    }

                    if (color.HasValue) ImGui.TextColored(color.Value, word);
                    else ImGui.Text(word);

                    first = false;
                }
            }
        }
    }

    private static void AchievementDescriptionTieredCurrentLevel(
        UnlockableAchievement? currentLevel, UnlockableAchievement maxLevel, bool progressLoaded, Configuration config
    ) {
        if (currentLevel == null || currentLevel == maxLevel) return;
        if (config.TieredAchievementDisplay == TieredAchievementDisplay.MaxOnly) return;

        WrappedColoredText((currentLevel.Description(), null), ("(current level)", UiColors.Grey()));

        // if the max is 0 or 1 we don't display any progress bars, except the second one if never hide progress bars is true
        if (maxLevel.Maximum() <= 1) return;

        ProgressBar(
            (maxLevel.Current() ?? 1.0f) / currentLevel.Maximum(),
            progressLoaded ? UiColors.Progress() : UiColors.Red(),
            height: UiSize.Em(config.ProgressBarHeight),
            insideText: progressLoaded ? $"{maxLevel.Current()}/{currentLevel.Maximum()}" : "Not loaded (click to refresh)",
            tooltip: "Click to refresh",
            enabled: progressLoaded,
            onClick: () => OpenAchievementWindow(maxLevel.Id()));
    }

    private static void AchievementDescriptionTieredMaxLevel(
        UnlockableAchievement? currentLevel, UnlockableAchievement maxLevel, bool progressLoaded, Configuration config
    ) {
        if (currentLevel != maxLevel && config.TieredAchievementDisplay == TieredAchievementDisplay.CurrentOnly) return;

        WrappedColoredText((maxLevel.Description(), null), ("(max level)", UiColors.Grey()));

        if (maxLevel.Unlocked() && !config.NeverHideProgressBars) return;
        if (maxLevel.Maximum() <= 1 && !config.NeverHideProgressBars) return;

        ProgressBar(
            (maxLevel.Current() ?? 1.0f) / maxLevel.Maximum(),
            progressLoaded ? UiColors.Progress() : UiColors.Red(),
            height: UiSize.Em(config.ProgressBarHeight),
            insideText: progressLoaded ? $"{maxLevel.Current()}/{maxLevel.Maximum()}" : "Not loaded (click to refresh)",
            tooltip: "Click to refresh",
            enabled: progressLoaded,
            onClick: () => OpenAchievementWindow(maxLevel.Id()));
    }

    private static void AchievementDescriptionTiered(UnlockableTieredAchievement achievements, Configuration config) {
        var currentLevel = achievements.ProvidesAchievements().Find(it => !it.Unlocked());
        var maxLevel = achievements.ProvidesAchievements().Last();
        var progressLoaded = maxLevel.Current() != null;

        AchievementDescriptionTieredCurrentLevel(currentLevel, maxLevel, progressLoaded, config);

        if (achievements.Spoilers() && currentLevel != null) return;

        AchievementDescriptionTieredMaxLevel(currentLevel, maxLevel, progressLoaded, config);
    }

    private static void TieredAchievementSimpleTiers(UnlockableTieredAchievement achievements, Configuration config) {
        var currentString = config.DisableRomanNumerals ? (achievements.Current() ?? 1).ToString() : ToRoman(achievements.Current() ?? 1);
        var maximumString = config.DisableRomanNumerals ? achievements.Maximum().ToString() : ToRoman(achievements.Maximum());
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(maximumString).X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextColored(achievements.ProvidesAchievements().Last().Unlocked() ? UiColors.Green() : UiColors.Red(), maximumString);
        position -= UiSize.Em(1) + ImGui.CalcTextSize("/").X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextDisabled("/");
        position -= UiSize.Em(1) + ImGui.CalcTextSize(currentString).X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextColored(UiColors.Green(), currentString);
    }

    private static void TieredAchievementTiers(UnlockableTieredAchievement achievements, Configuration config) {
        var widthCalculationText = "";
        for (var i = 1; i <= achievements.Maximum(); i++) {
            widthCalculationText += (config.DisableRomanNumerals ? i.ToString() : $"{ToRoman(i)}");
        }
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(widthCalculationText).X -
                       UiSize.Em(achievements.Maximum() - 1);

        for (var i = 1; i <= achievements.Maximum(); i++) {
            var text = config.DisableRomanNumerals ? i.ToString() : $"{ToRoman(i)}";

            ImGui.SameLine();
            ImGui.SetCursorPosX(position);
            ImGui.TextColored(achievements.ProvidesAchievements()[i - 1].Unlocked() ? UiColors.Green() : UiColors.Red(), text);
            if (i != achievements.Maximum()) {
                position += UiSize.Em(1) + ImGui.CalcTextSize(text).X;
            }
        }
    }

    public static void Achievement(UnlockableAchievement achievement, Configuration config) {
        ImGui.BeginGroup();

        AchievementIcon(achievement.Icon(), AchievementIconSize());
        ImGui.SameLine();
        ImGui.BeginGroup();
        AchievementHeaderTitle(
            achievement.Name(),
            config.DisplayIds ? achievement.Id() : null,
            config.PinnedAchievements.Contains(achievement.Id()),
            [achievement.Id()],
            config);
        AchievementHeaderLine1(() => SameLineRightTextColored(achievement.Unlocked() ? UiColors.Green() : UiColors.Red(),
                                                              achievement.Unlocked() ? "Unlocked" : "Locked"));
        AchievementHeaderLine2($"{achievement.Points()} points");
        ImGui.EndGroup();

        AchievementDescriptionSimple(achievement, config);

        ImGui.EndGroup();
    }

    public static void Achievement(UnlockableTieredAchievement achievements, Configuration config) {
        ImGui.BeginGroup();

        var maxLevel = achievements.ProvidesAchievements().Last();
        AchievementIcon(maxLevel.Icon(), AchievementIconSize());
        ImGui.SameLine();
        ImGui.BeginGroup();
        AchievementHeaderTitle(
            achievements.Name(),
            config.DisplayIds ? maxLevel.Id() : null,
            config.PinnedAchievements.Contains(achievements.Id()),
            achievements.Ids(),
            config);
        AchievementHeaderLine1(() => AchievementRightHeaderTiered(achievements, config));
        AchievementHeaderLine2($"{achievements.CurrentPoints()}/{achievements.MaximumPoints()} points");
        ImGui.EndGroup();

        AchievementDescriptionTiered(achievements, config);

        ImGui.EndGroup();
    }
}
