using System.Linq;
using System.Numerics;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views;

public class OverviewView(Plugin plugin, UnlockablesState unlockables, NavigationState navigation) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";

    private static void DrawCenteredWarning(string text) {
        var available = ImGui.GetContentRegionAvail();
        var textSize = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorPos();

        ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
        ImGui.TextColored(UiColors.Red(), text);
    }

    private void DrawTestNavigationButton() {
        var target = unlockables.FilteredLayout.AchievementLayout.FirstOrDefault()?.FindFirstCategory();
        if (target == null) return;

        if (ImGui.Button($"Go to \"{target.Name}\" (test)")) {
            navigation.Navigate(new NavigationTarget.Category(target.Id));
        }
    }

    public void Draw() {
        var ySize = UiSize.MainContentHeight(plugin.Configuration);
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
