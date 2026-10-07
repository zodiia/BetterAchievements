using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Recollection.Data;
using Recollection.Data.Unlockable;
using Recollection.UI.Component;

namespace Recollection.UI.Windows.Views;

public class AchievementsView(Plugin plugin, string breadcrumb, List<IUnlockable> unlockables, AchievementProgress progress, VariableHeightClipper clipper) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";

    private void DrawHeaderStatsLine(uint obtainedCount, uint totalCount, uint obtainedPoints, uint totalPoints) {
        var lineStartX = ImGui.GetCursorPosX();
        var lineStartY = ImGui.GetCursorPosY();
        var avail = ImGui.GetContentRegionAvail().X;

        ImGui.TextColored(UiColors.Blue(), obtainedCount.ToString());
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
        var (obtainedCount, totalCount) = progress.Count;
        var (obtainedPoints, totalPoints) = progress.Progress;
        var progressRatio = totalPoints == 0 ? 0f : (float)obtainedPoints / totalPoints;

        ImGui.Dummy(new(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));

        UiComponents.SeparatorText(breadcrumb, plugin.Configuration.UiDensity);

        DrawHeaderStatsLine(obtainedCount, totalCount, obtainedPoints, totalPoints);

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + UiSize.Em(0.5f));
        UiComponents.ProgressBar(progressRatio, UiColors.Progress(), insideText: $"{progressRatio * 100 :0.#}%");

        UiComponents.SeparatorText("Achievements", plugin.Configuration.UiDensity);
    }

    private bool DrawWarnings() {
        if (!Plugin.UnlockState.IsAchievementListLoaded) {
            var available = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(AchievementListNotLoadedWarning);
            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + ((available.X - textSize.X) / 2), Y = cursorPos.Y + ((available.Y - textSize.Y) / 2) });
            ImGui.TextColored(UiColors.Red(), AchievementListNotLoadedWarning);
            return true;
        }

        return false;
    }

    private void DrawAchievementsMainContent() {
        clipper.Draw(unlockables.Count, i => {
            switch (unlockables[i]) {
                case UnlockableAchievement achievement:
                    UiComponents.Achievement(achievement, plugin.Configuration);
                    break;
                case UnlockableTieredAchievement tiered:
                    UiComponents.Achievement(tiered, plugin.Configuration);
                    break;
            }

            if (i != unlockables.Count - 1) {
                ImGui.Separator();
            }
        });
    }

    public void Draw() {
        var ySize = ImGui.GetContentRegionAvail().Y;
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
