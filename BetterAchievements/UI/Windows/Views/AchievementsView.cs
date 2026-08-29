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

    private void DrawHeaderTitleLine(uint obtained, uint total) {
        using var font = UiFonts.FontSize125().Push();

        ImGui.TextUnformatted(breadcrumb);

        var obtainedText = obtained.ToString();
        var totalText = $" / {total} pts";
        var rightWidth = ImGui.CalcTextSize(obtainedText).X + ImGui.CalcTextSize(totalText).X;
        var targetX = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - rightWidth;

        ImGui.SameLine();
        ImGui.SetCursorPosX(targetX);
        ImGui.TextColored(UiColors.Orange(), obtainedText);
        ImGui.SameLine(0, 0);
        ImGui.TextUnformatted(totalText);
    }

    private void DrawHeader() {
        var (obtained, total) = ComputePoints();
        var progress = total == 0 ? 0f : (float)obtained / total;

        UiComponents.Callout(UiColors.Orange(), () => {
            DrawHeaderTitleLine(obtained, total);

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + UiSize.Em(0.5f));
            UiComponents.ProgressBar(progress, UiColors.Progress(), insideText: $"{(progress * 100).ToString("0.#")}%");
        });

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
