using System.Linq;
using System.Numerics;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.Unlockables;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents
{
    private static string ToRoman(int number)
    {
        if (number >= 10) return "X" + ToRoman(number - 10);
        if (number >= 9) return "IX" + ToRoman(number - 9);
        if (number >= 5) return "V" + ToRoman(number - 5);
        if (number >= 4) return "IV" + ToRoman(number - 4);
        if (number >= 1) return "I" + ToRoman(number - 1);
        return "";
    }

    public static void SameLineRightTextColored(Vector4 color, string text)
    {
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextColored(color, text);
    }

    private static void AchievementBase(UnlockableAchievement achievement)
    {
        ImGui.TextColored(UiColors.ColorOrange(), achievement.Name());
        ImGui.SameLine();
        ImGui.TextColored(UiColors.ColorYellow(), $" {achievement.Points()} points");
        ImGui.SameLine();
        ImGui.TextDisabled(" #" + achievement.Id());
        ImGui.SameLine();
        if (achievement.Unlocked())
        {
            SameLineRightTextColored(UiColors.ColorGreen(), "Unlocked");
        }
        else
        {
            SameLineRightTextColored(UiColors.ColorRed(), "Locked");
        }
        ImGui.TextWrapped(achievement.Description());
    }

    public static void SimpleAchievement(UnlockableAchievement achievement)
    {
        ImGui.BeginGroup();

        AchievementBase(achievement);

        ImGui.EndGroup();
    }

    public static void ProgressBasedAchievement(UnlockableAchievement achievement)
    {
        ImGui.BeginGroup();

        AchievementBase(achievement);

        var progress = achievement.Current();
        if (!achievement.Unlocked())
        {
            ProgressBar(
                (progress ?? 1.0f) / achievement.Maximum(),
                progress != null ? UiColors.ColorProgress().Brightness(0.5f) : UiColors.ColorRed(),
                insideText: progress != null ? $"{achievement.Current()}/{achievement.Maximum()}" : "Not loaded (click to refresh)",
                tooltip: "Click to refresh",
                enabled: progress != null,
                onClick: RequestAchievementProgress);
        }

        ImGui.EndGroup();
        return;

        unsafe void RequestAchievementProgress() => Plugin.UiState->Achievement.RequestAchievementProgress(achievement.Id());
    }

    private static void MultiProgressBasedAchievementLevels(UnlockableTieredAchievement achievements)
    {
        var widthCalculationText = "";
        for (var i = 1; i <= achievements.Maximum(); i++) widthCalculationText += ToRoman(i);
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(widthCalculationText).X;

        for (var i = 1; i <= achievements.Maximum(); i++)
        {
            var text = $"{ToRoman(i)}";
            position += i > 1 ? UiSize.Em(1.0f) : 0.0f;

            ImGui.SameLine();
            ImGui.SetCursorPosX(position);
            ImGui.TextColored(achievements.ProvidesAchievements()[i - 1].Unlocked() ? UiColors.ColorGreen() : UiColors.ColorRed(), text);
        }
    }

    public static void MultiProgressBasedAchievement(UnlockableTieredAchievement achievements)
    {
        ImGui.BeginGroup();

        ImGui.TextColored(UiColors.ColorOrange(), achievements.Name());
        ImGui.SameLine();
        ImGui.TextColored(UiColors.ColorYellow(), $" {achievements.CurrentPoints()}/{achievements.MaximumPoints()} points");
        MultiProgressBasedAchievementLevels(achievements);

        var currentLevel = achievements.ProvidesAchievements().Find(it => !it.Unlocked());
        var maxLevel = achievements.ProvidesAchievements().Last();
        var progressLoaded = maxLevel.Current() != null;

        // Current level
        if (currentLevel != null && currentLevel != maxLevel)
        {
            ImGui.Text(currentLevel.Description());
            ImGui.SameLine();
            ImGui.TextDisabled(" (current level)");
            ProgressBar(
                (maxLevel.Current() ?? 1.0f) / currentLevel.Maximum(),
                progressLoaded ? UiColors.ColorProgress().Brightness(0.5f) : UiColors.ColorRed(),
                insideText: progressLoaded ? $"{maxLevel.Current()}/{currentLevel.Maximum()}" : "Not loaded (click to refresh)",
                tooltip: "Click to refresh",
                enabled: progressLoaded,
                onClick: RequestAchievementProgress);
        }

        // Max level
        ImGui.Text(maxLevel.Description());
        ImGui.SameLine();
        ImGui.TextDisabled(" (max level)");

        if (!maxLevel.Unlocked())
        {
            ProgressBar(
                (maxLevel.Current() ?? 1.0f) / maxLevel.Maximum(),
                progressLoaded ? UiColors.ColorProgress().Brightness(0.5f) : UiColors.ColorRed(),
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
