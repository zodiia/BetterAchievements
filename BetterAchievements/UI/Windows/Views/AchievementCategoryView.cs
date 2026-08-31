using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using BetterAchievements.UI.Windows.Views.Overview;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views;

public class AchievementCategoryView(Plugin plugin, AchievementLayoutGroup group, UnlockablesState unlockables, NavigationState navigation) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";

    private static void DrawCenteredWarning(string text) {
        var available = ImGui.GetContentRegionAvail();
        var textSize = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorPos();

        ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
        ImGui.TextColored(UiColors.Red(), text);
    }

    private void DrawPinnedAchievements() {
        var pinned = unlockables.PinnedUnlockables(group);
        if (pinned.Count == 0) return;

        UiComponents.SeparatorText("Pinned");

        for (var i = 0; i < pinned.Count; i++) {
            switch (pinned[i]) {
                case UnlockableAchievement achievement:
                    UiComponents.Achievement(achievement, plugin.Configuration);
                    break;
                case UnlockableTieredAchievement tiered:
                    UiComponents.Achievement(tiered, plugin.Configuration);
                    break;
            }

            if (i != pinned.Count - 1) {
                ImGui.Separator();
            }
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

        ImGui.Dummy(new Vector2(0, UiSize.Em(1f)));

        UiComponents.SeparatorText(group.Name);
        OverviewComponents.OverviewStats(plugin, unlockables, group);

        UiComponents.SeparatorText("Categories");
        OverviewComponents.CategoriesGrid(unlockables, navigation, group.Items);

        DrawPinnedAchievements();

        ImGui.EndChild();
    }
}
