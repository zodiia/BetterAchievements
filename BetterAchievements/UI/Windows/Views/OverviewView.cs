using System.Numerics;
using BetterAchievements.UI.Component;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views;

public class OverviewView(Configuration configuration) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";
    private const string NoCategorySelectedWarning = "Please select a category.";

    private static void DrawCenteredWarning(string text) {
        var available = ImGui.GetContentRegionAvail();
        var textSize = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorPos();

        ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
        ImGui.TextColored(UiColors.Red(), text);
    }

    public void Draw() {
        var ySize = ImGui.GetContentRegionAvail().Y - (configuration.DebugMode ? 32 : 0);
        if (!ImGui.BeginChild("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true)) {
            return;
        }

        DrawCenteredWarning(!Plugin.UnlockState.IsAchievementListLoaded ? AchievementListNotLoadedWarning : NoCategorySelectedWarning);

        ImGui.EndChild();
    }
}
