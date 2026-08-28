using System.Collections.Generic;
using System.Numerics;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.Component;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views;

public class AchievementsView(int categoryId, List<IUnlockable> unlockables, Configuration configuration, VariableHeightClipper clipper) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";

    public int CategoryId => categoryId;

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
        // using (UiFonts.FontSize125().Push()) ImGui.TextColoredWrapped(UiColors.Purple(), "Battle / Field Operations");

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

        DrawAchievementsMainContent();

        ImGui.EndChild();
    }
}
