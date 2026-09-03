using System.Numerics;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views.Overview;

public class OverviewView(Plugin plugin, UnlockablesState unlockables, NavigationState navigation) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";

    private static void DrawCenteredWarning(string text) {
        var available = ImGui.GetContentRegionAvail();
        var textSize = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorPos();

        ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
        ImGui.TextColored(UiColors.Red(), text);
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

        ImGui.Dummy(new(0, UiSize.Em(1f)));

        // OverviewComponents.PlayerHeader();

        UiComponents.SeparatorText("Overview");
        OverviewComponents.OverviewStats(unlockables);

        UiComponents.SeparatorText("Categories");
        OverviewComponents.CategoriesGrid(unlockables, navigation);

        OverviewComponents.ActivityColumns(plugin, unlockables);

        ImGui.EndChild();
    }
}
