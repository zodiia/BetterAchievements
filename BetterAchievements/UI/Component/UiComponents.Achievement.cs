using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BetterAchievements.Data.Unlockable;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Component;

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

    public static void SameLineRightTextColored(Vector4 color, string text) {
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextColored(color, text);
    }

    private static void Pin(bool active, IEnumerable<uint> ids, Configuration configuration) {
        var color = active ? UiColors.Orange() : UiColors.Grey();
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

    private static void AchievementHeaderLine1(string name, uint? displayId, Action drawRightSide) {
        ImGui.TextColored(UiColors.Orange(), name);
        if (displayId.HasValue) {
            ImGui.SameLine();
            ImGui.TextDisabled(" #" + displayId.Value);
        }

        drawRightSide();
    }

    private static void AchievementRightHeaderTiered(UnlockableTieredAchievement achievements) {
        if (achievements.Maximum() >= 14) {
            TieredAchievementSimpleTiers(achievements);
        } else {
            TieredAchievementTiers(achievements);
        }
    }

    private static void AchievementHeaderLine2(string pointsText) {
        // placeholder for later features?
        ImGui.Dummy(Vector2.Zero);
        SameLineRightTextColored(UiColors.Orange(), pointsText);
    }

    private static void AchievementDescriptionSimple(UnlockableAchievement achievement) {
        ImGui.TextWrapped(achievement.Description());

        if (achievement.Maximum() <= 1 || achievement.Unlocked()) return;

        var progress = achievement.Current();

        ProgressBar(
            (progress ?? 1.0f) / achievement.Maximum(),
            progress != null ? UiColors.Progress() : UiColors.Red(),
            insideText: progress != null ? $"{achievement.Current()}/{achievement.Maximum()}" : "Not loaded (click to refresh)",
            tooltip: "Click to refresh",
            enabled: progress != null,
            onClick: RequestAchievementProgress);
        return;

        unsafe void RequestAchievementProgress() => Plugin.UiState->Achievement.RequestAchievementProgress(achievement.Id());
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

    private static void AchievementDescriptionTiered(UnlockableTieredAchievement achievements, Configuration configuration) {
        var currentLevel = achievements.ProvidesAchievements().Find(it => !it.Unlocked());
        var maxLevel = achievements.ProvidesAchievements().Last();
        var progressLoaded = maxLevel.Current() != null;

        // Current level
        if (currentLevel != null && currentLevel != maxLevel) {
            WrappedColoredText((currentLevel.Description(), null), ("(current level)", UiColors.Grey()));
            if (maxLevel.Maximum() > 1) {
                ProgressBar(
                    (maxLevel.Current() ?? 1.0f) / currentLevel.Maximum(),
                    progressLoaded ? UiColors.Progress() : UiColors.Red(),
                    insideText: progressLoaded ? $"{maxLevel.Current()}/{currentLevel.Maximum()}" : "Not loaded (click to refresh)",
                    tooltip: "Click to refresh",
                    enabled: progressLoaded,
                    onClick: RequestAchievementProgress);
            }
        }

        // Max level
        if (!achievements.Spoilers() || currentLevel == null) {
            WrappedColoredText((maxLevel.Description(), null), ("(max level)", UiColors.Grey()));

            if ((!maxLevel.Unlocked() && maxLevel.Maximum() > 1) || (configuration.NeverHideProgressBars)) {
                ProgressBar(
                    (maxLevel.Current() ?? 1.0f) / maxLevel.Maximum(),
                    progressLoaded ? UiColors.Progress() : UiColors.Red(),
                    insideText: progressLoaded ? $"{maxLevel.Current()}/{maxLevel.Maximum()}" : "Not loaded (click to refresh)",
                    tooltip: "Click to refresh",
                    enabled: progressLoaded,
                    onClick: RequestAchievementProgress);
            }
        }

        return;

        unsafe void RequestAchievementProgress() => Plugin.UiState->Achievement.RequestAchievementProgress(maxLevel.Id());
    }

    private static void TieredAchievementSimpleTiers(UnlockableTieredAchievement achievements) {
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(ToRoman(achievements.Maximum())).X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextColored(achievements.ProvidesAchievements().Last().Unlocked() ? UiColors.Green() : UiColors.Red(), ToRoman(achievements.Maximum()));
        position -= UiSize.Em(1) + ImGui.CalcTextSize("/").X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextDisabled("/");
        position -= UiSize.Em(1) + ImGui.CalcTextSize(ToRoman(achievements.Current() ?? 1)).X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextColored(UiColors.Green(), ToRoman(achievements.Current() ?? 1));
    }

    private static void TieredAchievementTiers(UnlockableTieredAchievement achievements) {
        var widthCalculationText = "";
        for (var i = 1; i <= achievements.Maximum(); i++) widthCalculationText += ToRoman(i);
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(widthCalculationText).X -
                       UiSize.Em(achievements.Maximum() - 1);

        for (var i = 1; i <= achievements.Maximum(); i++) {
            var text = $"{ToRoman(i)}";

            ImGui.SameLine();
            ImGui.SetCursorPosX(position);
            ImGui.TextColored(achievements.ProvidesAchievements()[i - 1].Unlocked() ? UiColors.Green() : UiColors.Red(), text);
            if (i != achievements.Maximum()) {
                position += UiSize.Em(1) + ImGui.CalcTextSize(text).X;
            }
        }
    }

    public static void Achievement(UnlockableAchievement achievement, Configuration configuration) {
        ImGui.BeginGroup();

        AchievementIcon(achievement.Icon(), AchievementIconSize());
        ImGui.SameLine();
        ImGui.BeginGroup();
        AchievementHeaderLine1(
            achievement.Name(),
            configuration.DisplayIds ? achievement.Id() : null,
            () => SameLineRightTextColored(achievement.Unlocked() ? UiColors.Green() : UiColors.Red(), achievement.Unlocked() ? "Unlocked" : "Locked"));
        Pin(configuration.PinnedAchievements.Contains(achievement.Id()), [achievement.Id()], configuration);
        AchievementHeaderLine2($"{achievement.Points()} points");
        ImGui.EndGroup();

        AchievementDescriptionSimple(achievement);

        ImGui.EndGroup();
    }

    public static void Achievement(UnlockableTieredAchievement achievements, Configuration configuration) {
        ImGui.BeginGroup();

        var maxLevel = achievements.ProvidesAchievements().Last();
        AchievementIcon(maxLevel.Icon(), AchievementIconSize());
        ImGui.SameLine();
        ImGui.BeginGroup();
        AchievementHeaderLine1(
            achievements.Name(),
            configuration.DisplayIds ? maxLevel.Id() : null,
            () => AchievementRightHeaderTiered(achievements));
        Pin(configuration.PinnedAchievements.Contains(achievements.Id()), achievements.Ids(), configuration);
        AchievementHeaderLine2($"{achievements.CurrentPoints()}/{achievements.MaximumPoints()} points");
        ImGui.EndGroup();

        AchievementDescriptionTiered(achievements, configuration);

        ImGui.EndGroup();
    }
}
