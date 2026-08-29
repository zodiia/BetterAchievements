using System.Linq;
using System.Numerics;
using BetterAchievements.UI.Component;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views;

public class OverviewView(MainWindowState state) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";

    private static void DrawCenteredWarning(string text) {
        var available = ImGui.GetContentRegionAvail();
        var textSize = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorPos();

        ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
        ImGui.TextColored(UiColors.Red(), text);
    }

    private void DrawTestNavigationButton() {
        var target = state.FilteredLayout.AchievementLayout.FirstOrDefault()?.FindFirstCategory();
        if (target == null) return;

        if (ImGui.Button($"Go to \"{target.Name}\" (test)")) {
            state.SetCategory(target.Id);
        }
    }

    public void Draw() {
        var ySize = ImGui.GetContentRegionAvail().Y - (state.Configuration.DebugMode ? 32 : 0);
        if (!ImGui.BeginChild("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true)) {
            return;
        }

        if (!Plugin.UnlockState.IsAchievementListLoaded) {
            DrawCenteredWarning(AchievementListNotLoadedWarning);
            ImGui.EndChild();
            return;
        }

        UiComponents.SeparatorText("Overview");
        DrawTestNavigationButton();

        ImGui.EndChild();
    }
}
