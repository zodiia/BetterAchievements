using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    private const string AchievementsNoun = "achievements";

    public static void OverviewStats(UnlockablesState unlockables) {
        OverviewStats(unlockables.ComputeOverallAchievementCount(), unlockables.ComputeOverallProgress(), AchievementsNoun);
    }

    public static void OverviewStats(UnlockablesState unlockables, AchievementLayout layout) {
        OverviewStats(unlockables.ComputeAchievementCount(layout), unlockables.ComputeProgress(layout), AchievementsNoun);
    }

    public static void OverviewStats(PointsScore count, string noun) {
        OverviewStats(count, null, noun);
    }

    private static void OverviewStats(PointsScore count, PointsScore? points, string noun) {
        var progress = ProgressRatio(count, points);

        DrawStatsLine(count, points, noun);

        ImGui.Dummy(new Vector2(0, UiSize.Em(0.35f)));
        UiComponents.ProgressBar(progress, UiColors.Progress(), insideText: $"{progress * 100:0.#}%");

        // ImGui.Dummy(new Vector2(0, UiSize.Em(0.35f)));
        // RanksRow(plugin);
    }

    private static float ProgressRatio(PointsScore count, PointsScore? points) {
        var score = points ?? count;
        return score.Total == 0 ? 0f : (float)score.Obtained / score.Total;
    }

    private static void DrawStatsLine(PointsScore count, PointsScore? points, string noun) {
        var lineStartX = ImGui.GetCursorPosX();
        var lineStartY = ImGui.GetCursorPosY();
        var avail = ImGui.GetContentRegionAvail().X;

        ImGui.TextColored(UiColors.Blue(), $"{count.Obtained:N0}");
        ImGui.SameLine(0, 0);
        ImGui.TextColored(UiColors.Text(), $" / {count.Total:N0} {noun}");

        if (points is not { } pointsScore) return;

        var obtainedText = $"{pointsScore.Obtained:N0}";
        var totalText = $" / {pointsScore.Total:N0} points";
        var rightWidth = ImGui.CalcTextSize(obtainedText).X + ImGui.CalcTextSize(totalText).X;

        ImGui.SameLine();
        ImGui.SetCursorPos(new Vector2 { X = lineStartX + avail - rightWidth, Y = lineStartY });
        ImGui.TextColored(UiColors.Progress(), obtainedText);
        ImGui.SameLine(0, 0);
        ImGui.TextColored(UiColors.Text(), totalText);
    }

    private static void RanksRow(Plugin plugin) {
        var location = GetPlayerLocation();

        RankBlock(location.World, plugin.LalachievementsService.GetWorldRank());
        ImGui.SameLine(0, UiSize.Em(1.5f));
        RankBlock(location.DataCenter, plugin.LalachievementsService.GetDataCenterRank());
        ImGui.SameLine(0, UiSize.Em(1.5f));
        RankBlock("Global", plugin.LalachievementsService.GetGlobalRank());
    }

    private static void RankBlock(string label, uint? rank) {
        ImGui.TextColored(UiColors.Grey(), label);
        ImGui.SameLine(0, UiSize.Em(0.35f));
        ImGui.TextColored(UiColors.Violet(), rank.HasValue ? $"#{rank.Value:N0}" : "-");
    }
}
