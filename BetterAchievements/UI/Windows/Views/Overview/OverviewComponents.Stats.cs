using System.Numerics;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    public static void OverviewStats(Plugin plugin, UnlockablesState unlockables) {
        var (obtainedCount, totalCount) = unlockables.ComputeOverallAchievementCount();
        var (obtainedPoints, totalPoints) = unlockables.ComputeOverallProgress();
        var progress = totalPoints == 0 ? 0f : (float)obtainedPoints / totalPoints;

        DrawStatsLine(obtainedCount, totalCount, obtainedPoints, totalPoints);

        ImGui.Dummy(new Vector2(0, UiSize.Em(0.35f)));
        UiComponents.ProgressBar(progress, UiColors.Progress(), insideText: $"{progress * 100:0.#}%");

        ImGui.Dummy(new Vector2(0, UiSize.Em(0.35f)));
        RanksRow(plugin);
    }

    private static void DrawStatsLine(uint obtainedCount, uint totalCount, uint obtainedPoints, uint totalPoints) {
        var lineStartX = ImGui.GetCursorPosX();
        var lineStartY = ImGui.GetCursorPosY();
        var avail = ImGui.GetContentRegionAvail().X;

        ImGui.TextColored(UiColors.Blue(), $"{obtainedCount:N0}");
        ImGui.SameLine(0, 0);
        ImGui.TextColored(UiColors.Text(), $" / {totalCount:N0} achievements");

        var obtainedText = $"{obtainedPoints:N0}";
        var totalText = $" / {totalPoints:N0} points";
        var rightWidth = ImGui.CalcTextSize(obtainedText).X + ImGui.CalcTextSize(totalText).X;

        ImGui.SameLine();
        ImGui.SetCursorPos(new Vector2 { X = lineStartX + avail - rightWidth, Y = lineStartY });
        ImGui.TextColored(UiColors.Progress(), obtainedText);
        ImGui.SameLine(0, 0);
        ImGui.TextColored(UiColors.Text(), totalText);
    }

    private static void RanksRow(Plugin plugin) {
        var location = GetPlayerLocation();

        // RankBlock(location.World, plugin.LalachievementsService.GetWorldRank());
        // ImGui.SameLine(0, UiSize.Em(1.5f));
        // RankBlock(location.DataCenter, plugin.LalachievementsService.GetDataCenterRank());
        // ImGui.SameLine(0, UiSize.Em(1.5f));
        // RankBlock("Global", plugin.LalachievementsService.GetGlobalRank());
    }

    private static void RankBlock(string label, uint? rank) {
        ImGui.TextColored(UiColors.Grey(), label);
        ImGui.SameLine(0, UiSize.Em(0.35f));
        ImGui.TextColored(UiColors.Violet(), rank.HasValue ? $"#{rank.Value:N0}" : "-");
    }
}
