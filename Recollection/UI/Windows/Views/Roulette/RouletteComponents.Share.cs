using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Bindings.ImGui;
using AchievementRoulette = Recollection.UI.State.AchievementRoulette;

namespace Recollection.UI.Windows.Views.Roulette;

public static partial class RouletteComponents {
    private const float CopiedFeedbackSeconds = 2f;
    private const int ShareProgressLength = 10;
    private const string SparklineCharacters = "▁▂▃▄▅▆▇█";

    private static long? CopiedRouletteId;
    private static DateTime CopiedAt;

    private static string ShareText(AchievementRoulette roulette) {
        var achievement = roulette.Achievement;
        var highest = Math.Max(1u, roulette.Daily.DefaultIfEmpty(0u).Max());
        var sparkline = string.Concat(
            roulette.Daily.Select(it => SparklineCharacters[(int)MathF.Round((float)it / highest * (SparklineCharacters.Length - 1))]));
        var lines = new List<string> {
            $"### :trophy: {achievement.Name} ({achievement.Points} points)",
            $"-# {achievement.Description}",
            "",
            $":calendar: Rolled on <t:{roulette.Roulette.RollDate}>",
        };

        if (roulette.Roulette.CompletionDate is { } completionDate) {
            lines.Add($":white_check_mark: Completed on <t:{completionDate}>");

            if (roulette.Maximum > 1) {
                lines.Add($":bar_chart: {sparkline} (~{roulette.PerDay:#,0.#} per day)");
            }
        } else {
            if (roulette.Maximum > 1) {
                var filled = (int)(roulette.Current * ShareProgressLength / roulette.Maximum);

                lines.Add($"{new string('▰', filled)}{new string('▱', ShareProgressLength - filled)}   {roulette.Current:N0} / {roulette.Maximum:N0}");
            }
        }

        return string.Join("\n", lines);
    }

    private static void ShareButton(AchievementRoulette achievementRoulette) {
        var copied = CopiedRouletteId == achievementRoulette.Roulette.Id && (DateTime.Now - CopiedAt).TotalSeconds < CopiedFeedbackSeconds;
        if (ImGui.Button(copied ? "Copied!##Share" : "Share##Share")) {
            ImGui.SetClipboardText(ShareText(achievementRoulette));
            CopiedRouletteId = achievementRoulette.Roulette.Id;
            CopiedAt = DateTime.Now;
        }

        if (!ImGui.IsItemHovered()) return;

        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        ImGui.SetTooltip("Copy a summary of this roulette to share it on Discord or elsewhere");
    }
}
