using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.Data.Unlockable;
using Recollection.Services;
using Recollection.UI.Component;
using Recollection.UI.State;
using Recollection.UI.Windows.Views.Overview;

namespace Recollection.UI.Windows.Views;

public class CollectionOverviewView(Plugin plugin, UnlockableType type, UnlockablesState unlockables, NavigationState navigation) : IView {
    public void Draw() {
        var ySize = ImGui.GetContentRegionAvail().Y;
        using var mainContent = ImRaii.Child("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true);
        if (!mainContent) return;

        ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));

        UiComponents.SeparatorText(CollectionsService.Label(type), plugin.Configuration.UiDensity);
        OverviewComponents.OverviewStats(plugin, unlockables.GetProgress(type).Score, CollectionsService.Label(type).ToLower());

        UiComponents.SeparatorText("Categories", plugin.Configuration.UiDensity);
        OverviewComponents.CollectionCategoriesGrid(plugin, unlockables, navigation, type);
    }
}
