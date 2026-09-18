using Dalamud.Bindings.ImGui;
using Recollection.Data;
using Recollection.UI.State;

namespace Recollection.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private const float NestedLevelIndentEm = 1f;

    private static void SubTree(MainWindowState state, AchievementLayout layout) {
        var (obtained, total) = state.Unlockables.ComputeProgress(layout);
        var progress = total == 0 ? 0f : (float)obtained / total;

        switch (layout) {
            case AchievementLayoutCategory category:
                var target = new NavigationTarget.Category(category.Id);
                if (SubCategoryRow($"##SubCategory-{category.Id}", category.Name, progress, state.Navigation.IsSelected(target))) {
                    state.Navigation.Navigate(target);
                }

                break;

            case AchievementLayoutGroup group:
                StaticSubCategoryLabel(group.Name, progress);

                ImGui.Indent(UiSize.Em(NestedLevelIndentEm));
                foreach (var item in group.Items) {
                    SubTree(state, item);
                }

                ImGui.Unindent(UiSize.Em(NestedLevelIndentEm));

                break;
        }
    }
}
