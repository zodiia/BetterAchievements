using System;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views;

public class AchievementCategoryView(Plugin plugin, AchievementLayoutGroup group, UnlockablesState unlockables, NavigationState navigation) : IView {
    private void DrawTestNavigationButton() {
        var target = group.FindFirstCategory();
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

        UiComponents.SeparatorText(group.Name);

        var (obtained, total) = unlockables.ComputeProgress(group);
        var progress = total == 0 ? 0f : (float)obtained / total;
        UiComponents.ProgressBar(progress, UiColors.Progress(), insideText: $"{(int)MathF.Round(progress * 100)}%");

        ImGui.Dummy(new Vector2(0, UiSize.Em(1f)));
        DrawTestNavigationButton();

        ImGui.EndChild();
    }
}
