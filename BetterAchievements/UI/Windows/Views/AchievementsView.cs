using System.Collections.Generic;
using System.Numerics;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.Component;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views;

public class AchievementsView(int categoryId, string breadcrumb, List<IUnlockable> unlockables, Configuration configuration, VariableHeightClipper clipper) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";

    public int CategoryId => categoryId;

    private (uint Obtained, uint Total) ComputePoints() {
        uint obtained = 0;
        uint total = 0;

        foreach (var unlockable in unlockables) {
            switch (unlockable) {
                case UnlockableAchievement achievement:
                    total += achievement.Points();
                    if (achievement.Unlocked()) obtained += achievement.Points();
                    break;
                case UnlockableTieredAchievement tiered:
                    total += tiered.MaximumPoints();
                    obtained += tiered.CurrentPoints();
                    break;
            }
        }

        return (obtained, total);
    }

    private (uint Obtained, uint Total) ComputeAchievementCounts() {
        uint obtained = 0;

        foreach (var unlockable in unlockables) {
            if (unlockable.Unlocked()) obtained++;
        }

        return (obtained, (uint)unlockables.Count);
    }

    private void DrawHeaderStatsLine(uint obtainedCount, uint totalCount, uint obtainedPoints, uint totalPoints) {
        var lineStartX = ImGui.GetCursorPosX();
        var lineStartY = ImGui.GetCursorPosY();
        var avail = ImGui.GetContentRegionAvail().X;

        ImGui.TextColored(UiColors.Blue(), obtainedCount.ToString()); // #fadf5a
        ImGui.SameLine(0, 0);
        ImGui.TextUnformatted($" / {totalCount} achievements");

        var obtainedText = obtainedPoints.ToString();
        var totalText = $" / {totalPoints} pts";
        var rightWidth = ImGui.CalcTextSize(obtainedText).X + ImGui.CalcTextSize(totalText).X;
        var targetX = lineStartX + avail - rightWidth;

        ImGui.SameLine();
        ImGui.SetCursorPos(new Vector2 { X = targetX, Y = lineStartY });
        ImGui.TextColored(UiColors.Progress(), obtainedText);
        ImGui.SameLine(0, 0);
        ImGui.TextUnformatted(totalText);
    }

    private void DrawHeader() {
        var (obtainedPoints, totalPoints) = ComputePoints();
        var (obtainedCount, totalCount) = ComputeAchievementCounts();
        var progress = totalPoints == 0 ? 0f : (float)obtainedPoints / totalPoints;

        UiComponents.SeparatorText(breadcrumb, paddingAboveEm: 0f);

        DrawHeaderStatsLine(obtainedCount, totalCount, obtainedPoints, totalPoints);

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + UiSize.Em(0.5f));
        UiComponents.ProgressBar(progress, UiColors.Progress(), insideText: $"{(progress * 100).ToString("0.#")}%");

        UiComponents.SeparatorText("Achievements");
    }

    private bool DrawWarnings() {
        if (!Plugin.UnlockState.IsAchievementListLoaded) {
            var available = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(AchievementListNotLoadedWarning);
            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
            ImGui.TextColored(UiColors.Red(), AchievementListNotLoadedWarning);
            return true;
        }

        return false;
    }

    private void DrawAchievementsMainContent() {
        clipper.Draw(unlockables.Count, i => {
            switch (unlockables[i]) {
                case UnlockableAchievement achievement:
                    UiComponents.Achievement(achievement, configuration);
                    break;
                case UnlockableTieredAchievement tiered:
                    UiComponents.Achievement(tiered, configuration);
                    break;
            }

            if (i != unlockables.Count - 1) {
                ImGui.Separator();
            }
        });
    }

    public void Draw() {
        var ySize = ImGui.GetContentRegionAvail().Y - (configuration.DebugMode ? 32 : 0);
        if (!ImGui.BeginChild("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true)) {
            return;
        }

        if (DrawWarnings()) {
            ImGui.EndChild();
            return;
        }

        DrawHeader();
        DrawAchievementsMainContent();

        ImGui.EndChild();
    }
}
