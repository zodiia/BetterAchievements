using BetterAchievements.Data;
using BetterAchievements.UI.Windows;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private const float NestedLevelIndentEm = 1f;

    private static void SubTree(MainWindowState state, AchievementLayout layout) {
        var (obtained, total) = state.ComputeProgress(layout);
        var progress = total == 0 ? 0f : (float)obtained / total;

        switch (layout) {
            case AchievementLayoutCategory category:
                if (SubCategoryRow($"##SubCategory-{category.Id}", category.Name, progress, state.SelectedCategoryId == category.Id)) {
                    state.SetCategory(category.Id);
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
