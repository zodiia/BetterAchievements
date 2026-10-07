using Dalamud.Bindings.ImGui;
using Recollection.Data;
using Recollection.UI.State;

namespace Recollection.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private const float NestedLevelIndentEm = 1f;

    private static void SubTree(Plugin plugin, MainWindowState state, AchievementLayout layout) {
        var (_, (obtained, total)) = state.Unlockables.GetProgress(layout);
        var progress = total == 0 ? 0f : (float)obtained / total;

        switch (layout) {
            case AchievementLayoutCategory category:
                var target = new NavigationTarget.Category(category.Id);
                if (SubCategoryRow(plugin, $"##SubCategory-{category.Id}", category.Name, progress, state.Navigation.IsSelected(target))) {
                    state.Navigation.Navigate(target);
                }

                break;

            case AchievementLayoutGroup group:
                StaticSubCategoryLabel(plugin, group.Name, progress);

                ImGui.Indent(UiSize.Em(NestedLevelIndentEm));
                foreach (var item in group.Items) {
                    SubTree(plugin, state, item);
                }

                ImGui.Unindent(UiSize.Em(NestedLevelIndentEm));

                break;
        }
    }
}
