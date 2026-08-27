using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.Windows;
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

    private static void Pin(bool active, IEnumerable<uint> ids, MainWindowState mainWindowState, Plugin plugin) {
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
                plugin.Configuration.PinnedAchievements.RemoveAll(ids.Contains);
            } else {
                plugin.Configuration.PinnedAchievements.Add(ids.Last());
            }

            plugin.Configuration.Save();
            mainWindowState.RefreshPinnedAchievements();
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

    private static void AchievementDescriptionTiered(UnlockableTieredAchievement achievements, MainWindowState mainWindowState) {
        var currentLevel = achievements.ProvidesAchievements().Find(it => !it.Unlocked());
        var maxLevel = achievements.ProvidesAchievements().Last();
        var progressLoaded = maxLevel.Current() != null;

        // Current level
        if (currentLevel != null && currentLevel != maxLevel) {
            ImGui.Text(currentLevel.Description());
            ImGui.SameLine();
            ImGui.TextDisabled(" (current level)");
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
            ImGui.Text(maxLevel.Description());
            ImGui.SameLine();
            ImGui.TextDisabled(" (max level)");

            if ((!maxLevel.Unlocked() && maxLevel.Maximum() > 1) || (mainWindowState.Configuration.NeverHideProgressBars)) {
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

    public static void Achievement(UnlockableAchievement achievement, MainWindowState mainWindowState, Plugin plugin) {
        ImGui.BeginGroup();

        AchievementIcon(achievement.Icon(), AchievementIconSize());
        ImGui.SameLine();
        ImGui.BeginGroup();
        AchievementHeaderLine1(
            achievement.Name(),
            mainWindowState.Configuration.DisplayIds ? achievement.Id() : null,
            () => SameLineRightTextColored(achievement.Unlocked() ? UiColors.Green() : UiColors.Red(), achievement.Unlocked() ? "Unlocked" : "Locked"));
        Pin(plugin.Configuration.PinnedAchievements.Contains(achievement.Id()), [achievement.Id()], mainWindowState, plugin);
        AchievementHeaderLine2($"{achievement.Points()} points");
        ImGui.EndGroup();

        AchievementDescriptionSimple(achievement);

        ImGui.EndGroup();
    }

    public static void Achievement(UnlockableTieredAchievement achievements, MainWindowState mainWindowState, Plugin plugin) {
        ImGui.BeginGroup();

        var maxLevel = achievements.ProvidesAchievements().Last();
        AchievementIcon(maxLevel.Icon(), AchievementIconSize());
        ImGui.SameLine();
        ImGui.BeginGroup();
        AchievementHeaderLine1(
            achievements.Name(),
            mainWindowState.Configuration.DisplayIds ? maxLevel.Id() : null,
            () => AchievementRightHeaderTiered(achievements));
        Pin(plugin.Configuration.PinnedAchievements.Contains(achievements.Id()), achievements.Ids(), mainWindowState, plugin);
        AchievementHeaderLine2($"{achievements.CurrentPoints()}/{achievements.MaximumPoints()} points");
        ImGui.EndGroup();

        AchievementDescriptionTiered(achievements, mainWindowState);

        ImGui.EndGroup();
    }
}
