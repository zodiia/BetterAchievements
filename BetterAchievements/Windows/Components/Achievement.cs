using System.Linq;
using System.Numerics;
using BetterAchievements.Unlockables;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Serilog;

namespace BetterAchievements.Windows.Components;

public static partial class ImGuiComponents
{
    public static void SameLineRightTextColored(Vector4 color, string text)
    {
        var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(position);
        ImGui.TextColored(color, text);
    }

    private static void AchievementBase(UnlockableAchievement achievement)
    {
        ImGui.TextColored(ColorOrange(), achievement.Name());
        ImGui.SameLine();
        ImGui.TextDisabled(" #" + achievement.Id());
        ImGui.SameLine();
        if (achievement.Unlocked)
        {
            SameLineRightTextColored(ColorGreen(), "Unlocked");
        }
        else
        {
            SameLineRightTextColored(ColorRed(), "Locked");
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

        var progress = achievement.Progress();
        if (!achievement.Unlocked)
        {
            ProgressBar(
                (progress ?? 1.0f) / achievement.Maximum() ?? 1.0f,
                progress != null ? ColorProgress().Brightness(0.5f) : ColorRed(),
                insideText: progress != null ? $"{achievement.Progress()}/{achievement.Maximum()}" : "Not loaded (click to refresh)",
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
        int i = achievements.Unlocked.Count;
        var currentText = "";
        while (i > 0)
        {
            var text = $"{ToRoman(i)}";
            currentText += text;
            var spacing = SizeEm(1.0f) * (achievements.Unlocked.Count - i);
            var position = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(currentText).X - spacing;

            ImGui.SameLine();
            ImGui.SetCursorPosX(position);
            ImGui.TextColored(achievements.Unlocked[i - 1] ? ColorGreen() : ColorRed(), text);

            i--;
        }
    }

    public static void MultiProgressBasedAchievement(UnlockableTieredAchievement achievements)
    {
        ImGui.BeginGroup();

        ImGui.TextColored(ColorOrange(), achievements.Name);
        MultiProgressBasedAchievementLevels(achievements);

        var currentLevel = achievements.UnlockableAchievements.Find(it => !it.Unlocked);
        var maxLevel = achievements.UnlockableAchievements.Last();
        var progressLoaded = maxLevel.Progress() != null;

        // Current level
        if (currentLevel != null && currentLevel != maxLevel)
        {
            ImGui.Text(currentLevel.Description());
            ImGui.SameLine();
            ImGui.TextDisabled(" (current level)");
            ProgressBar(
                (maxLevel.Progress() ?? 1.0f) / currentLevel.Maximum() ?? 1.0f,
                progressLoaded ? ColorProgress().Brightness(0.5f) : ColorRed(),
                insideText: progressLoaded ? $"{maxLevel.Progress()}/{currentLevel.Maximum()}" : "Not loaded (click to refresh)",
                tooltip: "Click to refresh",
                enabled: progressLoaded,
                onClick: RequestAchievementProgress);
        }

        // Max level
        ImGui.Text(maxLevel.Description());
        ImGui.SameLine();
        ImGui.TextDisabled(" (max level)");

        if (!maxLevel.Unlocked)
        {
            ProgressBar(
                (maxLevel.Progress() ?? 1.0f) / maxLevel.Maximum() ?? 1.0f,
                progressLoaded ? ColorProgress().Brightness(0.5f) : ColorRed(),
                insideText: progressLoaded ? $"{maxLevel.Progress()}/{maxLevel.Maximum()}" : "Not loaded (click to refresh)",
                tooltip: "Click to refresh",
                enabled: progressLoaded,
                onClick: RequestAchievementProgress);
        }

        ImGui.EndGroup();
        return;

        unsafe void RequestAchievementProgress() => Plugin.UiState->Achievement.RequestAchievementProgress(maxLevel.Id());
    }
}
