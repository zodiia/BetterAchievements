using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.Windows;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents
{
    private static string ToRoman(uint number) => ToRoman((int)number);

    private static string ToRoman(int number)
    {
        return number switch
        {
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

    public static void SameLineRightTextColored(Vector4 color, string text)
    {
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextColored(color, text);
    }

    private static void Pin(bool active, IEnumerable<uint> ids, MainWindowState mainWindowState, Plugin plugin)
    {
        var color = active ? UiColors.Orange() : UiColors.Grey();
        var hoverText = active ? "Unpin this achievement" : "Pin this achievement";
        var icon = active ? FontAwesomeIcon.Thumbtack : FontAwesomeIcon.ThumbtackSlash;
        var boxStart = ImGui.GetCursorScreenPos();
        Vector2 boxEnd;

        using (var _ = ImRaii.PushFont(UiBuilder.IconFont))
        {
            var iconText = icon.ToIconString(); // this one is bigger
            var textSize = ImGui.CalcTextSize(iconText);
            boxEnd = new Vector2(boxStart.X + textSize.X, boxStart.Y + textSize.Y);
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 1); // for some reason it clips by 1px by default
            ImGui.TextColored(color, iconText);
            ImGui.SameLine();
            if (active) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3); // necessary readjustments
        }
        if (ImGui.IsMouseHoveringRect(boxStart, boxEnd))
        {
            ImGui.SetTooltip(hoverText);
        }
        if (ImGui.IsItemClicked())
        {
            if (active)
            {
                plugin.Configuration.PinnedAchievements.RemoveAll(ids.Contains);
            }
            else
            {
                plugin.Configuration.PinnedAchievements.Add(ids.Last());
            }
            plugin.Configuration.Save();
            mainWindowState.RefreshPinnedAchievements();
        }
    }

    private static void AchievementBase(UnlockableAchievement achievement, MainWindowState mainWindowState, Plugin plugin)
    {
        Pin(plugin.Configuration.PinnedAchievements.Contains(achievement.Id()), [achievement.Id()], mainWindowState, plugin);

        ImGui.TextColored(UiColors.Orange(), achievement.Name());
        ImGui.SameLine();
        ImGui.TextColored(UiColors.Yellow(), $" {achievement.Points()} points");
        ImGui.SameLine();
        ImGui.TextDisabled(" #" + achievement.Id());
        ImGui.SameLine();
        if (achievement.Unlocked())
        {
            SameLineRightTextColored(UiColors.Green(), "Unlocked");
        }
        else
        {
            SameLineRightTextColored(UiColors.Red(), "Locked");
        }
        ImGui.TextWrapped(achievement.Description());
    }

    public static void SimpleAchievement(UnlockableAchievement achievement, MainWindowState mainWindowState, Plugin plugin)
    {
        ImGui.BeginGroup();

        AchievementBase(achievement, mainWindowState, plugin);

        ImGui.EndGroup();
    }

    public static void ProgressBasedAchievement(UnlockableAchievement achievement, MainWindowState mainWindowState, Plugin plugin)
    {
        ImGui.BeginGroup();

        AchievementBase(achievement, mainWindowState, plugin);

        var progress = achievement.Current();
        if (!achievement.Unlocked())
        {
            ProgressBar(
                (progress ?? 1.0f) / achievement.Maximum(),
                progress != null ? UiColors.Progress().Brightness(0.5f) : UiColors.Red(),
                insideText: progress != null ? $"{achievement.Current()}/{achievement.Maximum()}" : "Not loaded (click to refresh)",
                tooltip: "Click to refresh",
                enabled: progress != null,
                onClick: RequestAchievementProgress);
        }

        ImGui.EndGroup();
        return;

        unsafe void RequestAchievementProgress() => Plugin.UiState->Achievement.RequestAchievementProgress(achievement.Id());
    }

    private static void TieredAchievementSimpleTiers(UnlockableTieredAchievement achievements)
    {
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

    private static void TieredAchievementTiers(UnlockableTieredAchievement achievements)
    {
        var widthCalculationText = "";
        for (var i = 1; i <= achievements.Maximum(); i++) widthCalculationText += ToRoman(i);
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(widthCalculationText).X - UiSize.Em(achievements.Maximum() - 1);

        for (var i = 1; i <= achievements.Maximum(); i++)
        {
            var text = $"{ToRoman(i)}";

            ImGui.SameLine();
            ImGui.SetCursorPosX(position);
            ImGui.TextColored(achievements.ProvidesAchievements()[i - 1].Unlocked() ? UiColors.Green() : UiColors.Red(), text);
            if (i != achievements.Maximum())
            {
                position += UiSize.Em(1) + ImGui.CalcTextSize(text).X;
            }
        }
    }

    public static void TieredAchievement(UnlockableTieredAchievement achievements, MainWindowState mainWindowState, Plugin plugin)
    {
        ImGui.BeginGroup();

        Pin(plugin.Configuration.PinnedAchievements.Contains(achievements.Id()), achievements.Ids(), mainWindowState, plugin);

        ImGui.TextColored(UiColors.Orange(), achievements.Name());
        ImGui.SameLine();
        ImGui.TextColored(UiColors.Yellow(), $" {achievements.CurrentPoints()}/{achievements.MaximumPoints()} points");
        if (achievements.Maximum() >= 14)
        {
            TieredAchievementSimpleTiers(achievements);
        }
        else
        {
            TieredAchievementTiers(achievements);
        }

        var currentLevel = achievements.ProvidesAchievements().Find(it => !it.Unlocked());
        var maxLevel = achievements.ProvidesAchievements().Last();
        var progressLoaded = maxLevel.Current() != null;

        // Current level
        if (currentLevel != null && currentLevel != maxLevel)
        {
            ImGui.Text(currentLevel.Description());
            ImGui.SameLine();
            ImGui.TextDisabled(" (current level)");
            if (maxLevel.Maximum() > 1)
            {
                ProgressBar(
                    (maxLevel.Current() ?? 1.0f) / currentLevel.Maximum(),
                    progressLoaded ? UiColors.Progress().Brightness(0.5f) : UiColors.Red(),
                    insideText: progressLoaded ? $"{maxLevel.Current()}/{currentLevel.Maximum()}" : "Not loaded (click to refresh)",
                    tooltip: "Click to refresh",
                    enabled: progressLoaded,
                    onClick: RequestAchievementProgress);
            }
        }

        // Max level
        ImGui.Text(maxLevel.Description());
        ImGui.SameLine();
        ImGui.TextDisabled(" (max level)");

        if (!maxLevel.Unlocked() && maxLevel.Maximum() > 1)
        {
            ProgressBar(
                (maxLevel.Current() ?? 1.0f) / maxLevel.Maximum(),
                progressLoaded ? UiColors.Progress().Brightness(0.5f) : UiColors.Red(),
                insideText: progressLoaded ? $"{maxLevel.Current()}/{maxLevel.Maximum()}" : "Not loaded (click to refresh)",
                tooltip: "Click to refresh",
                enabled: progressLoaded,
                onClick: RequestAchievementProgress);
        }

        ImGui.EndGroup();
        return;

        unsafe void RequestAchievementProgress() => Plugin.UiState->Achievement.RequestAchievementProgress(maxLevel.Id());
    }
}
