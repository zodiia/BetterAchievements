using System.Numerics;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.Services;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using BetterAchievements.UI.Windows.Views.Overview;
using Dalamud.Bindings.ImGui;

using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Windows.Views;

public class CollectionOverviewView(Plugin plugin, UnlockableType type, UnlockablesState unlockables, NavigationState navigation) : IView {
    public void Draw() {
        var ySize = UiSize.MainContentHeight(plugin.Configuration);
        using var mainContent = ImRaii.Child("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true);
        if (!mainContent) return;

        ImGui.Dummy(new Vector2(0, UiSize.Em(1f)));

        UiComponents.SeparatorText(CollectionsService.Label(type));
        OverviewComponents.OverviewStats(unlockables.ComputeProgress(type).Score, CollectionsService.Label(type).ToLower());

        UiComponents.SeparatorText("Categories");
        OverviewComponents.CollectionCategoriesGrid(unlockables, navigation, type);
    }
}
