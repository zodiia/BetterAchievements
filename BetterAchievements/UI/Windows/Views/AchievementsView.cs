using System.Linq;
using System.Numerics;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.Component;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views;

public class AchievementsView(MainWindowState state, Plugin plugin) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";
    private const string NoCategorySelectedWarning = "Please select a category.";

    private bool DrawWarnings() {
        if (!Plugin.UnlockState.IsAchievementListLoaded) {
            var available = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(AchievementListNotLoadedWarning);
            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
            ImGui.TextColored(UiColors.Red(), AchievementListNotLoadedWarning);
            return true;
        }

        if (state.SelectedCategoryId == MainWindowState.NoCategoryId) {
            var available = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(NoCategorySelectedWarning);
            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new() { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
            ImGui.TextColored(UiColors.Red(), NoCategorySelectedWarning);
            return true;
        }

        return false;
    }

    private void DrawAchievementsMainContent() {
        foreach (var it in state.CategoryUnlockables) {
            switch (it) {
                case UnlockableAchievement achievement:
                    UiComponents.Achievement(achievement, state, plugin);
                    break;
                case UnlockableTieredAchievement tiered:
                    UiComponents.Achievement(tiered, state, plugin);
                    break;
            }

            if (it != state.CategoryUnlockables.Last()) {
                ImGui.Separator();
            }
        }
    }

    public void Draw() {
        var ySize = ImGui.GetContentRegionAvail().Y - (state.Configuration.DebugMode ? 32 : 0);
        if (!ImGui.BeginChild("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true)) {
            return;
        }

        if (DrawWarnings() || state.SelectedAchievementCategory == null) // null is already checked but just doing that so that my ide stops screaming at me
        {
            ImGui.EndChild();
            return;
        }

        DrawAchievementsMainContent();

        ImGui.EndChild();
    }
}
