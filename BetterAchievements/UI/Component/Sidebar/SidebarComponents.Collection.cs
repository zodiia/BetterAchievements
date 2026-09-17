using System.Numerics;
using BetterAchievements.Services;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace BetterAchievements.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private static FontAwesomeIcon CollectionIcon(UnlockableType type) {
        return type switch {
            UnlockableType.Mount => FontAwesomeIcon.Horse,
            UnlockableType.Minion => FontAwesomeIcon.Cat,
            UnlockableType.Title => FontAwesomeIcon.Signature,
            UnlockableType.TripleTriadCard => FontAwesomeIcon.Clone,
            UnlockableType.TripleTriadNpc => FontAwesomeIcon.UserFriends,
            UnlockableType.Barding => FontAwesomeIcon.HatCowboy,
            UnlockableType.FashionAccessory => FontAwesomeIcon.Hiking,
            UnlockableType.Hairstyle => FontAwesomeIcon.Cut,
            UnlockableType.Facewear => FontAwesomeIcon.Glasses,
            UnlockableType.Emote => FontAwesomeIcon.Smile,
            UnlockableType.CraftingLog => FontAwesomeIcon.Hammer,
            UnlockableType.GatheringLog => FontAwesomeIcon.Gem,
            _ => FontAwesomeIcon.Question,
        };
    }

    private static void CollectionsLoading() {
        ImGui.Indent(UiSize.Em(CategoryRowPaddingEm));
        ImGui.TextColored(UiColors.Grey(), "Loading...");
        ImGui.Unindent(UiSize.Em(CategoryRowPaddingEm));
        ImGui.Dummy(new Vector2(0, UiSize.Em(SubTreeBottomPaddingEm)));
    }

    private static void CollectionCategories(MainWindowState state, UnlockableType type) {
        ImGui.Indent(UiSize.Em(FirstLevelIndentEm));

        foreach (var category in state.Unlockables.CollectionCategories(type)) {
            var (score, visible) = state.Unlockables.ComputeProgress(type, category);
            if (visible == 0) continue;

            var target = new NavigationTarget.CollectionCategory(type, category.Id);
            var progress = score.Total == 0 ? 0f : (float)score.Obtained / score.Total;

            if (SubCategoryRow($"##CollectionCategory-{type}-{category.Id}", category.Name, progress, state.Navigation.IsSelected(target))) {
                state.Navigation.Navigate(target);
            }
        }

        ImGui.Unindent(UiSize.Em(FirstLevelIndentEm));
        ImGui.Dummy(new Vector2(0, UiSize.Em(SubTreeBottomPaddingEm)));
    }

    private static void CollectionItem(Plugin plugin, MainWindowState state, UnlockableType type, Vector4 color) {
        var progress = state.Unlockables.ComputeProgress(type);
        if (progress.VisibleCount == 0) return;

        var target = new NavigationTarget.Collection(type);
        var isOpen = state.Navigation.IsGroupOpen(CollectionsService.Label(type));
        var selected = state.Navigation.IsSelected(target);
        var progressPercentage = progress.Score.Total == 0 ? 0f : (float)progress.Score.Obtained / progress.Score.Total;

        if (CategoryRow(plugin, $"##Collection-{type}", CollectionIcon(type), CollectionsService.Label(type), progressPercentage, null, color, isOpen, selected)) {
            state.Navigation.Navigate(target);
        }

        if (isOpen && type != UnlockableType.Title) {
            CollectionCategories(state, type);
        }
    }
}
