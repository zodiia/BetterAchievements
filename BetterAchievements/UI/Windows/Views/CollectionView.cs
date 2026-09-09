using System.Collections.Generic;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.Services;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.Windows.Views.Overview;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Windows.Views;

public class CollectionView(
    Plugin plugin,
    UnlockableType type,
    string breadcrumb,
    List<IUnlockable> entries,
    PointsScore score,
    VariableHeightClipper clipper) : IView {
    private const string TitleListNotLoadedWarning = "Title list not loaded, please open the vanilla title window once!\n(Character Window > Profile > Title > Acquired Titles)";

    private void DrawHeader() {
        UiComponents.SeparatorText(breadcrumb, paddingAboveEm: 0f);
        OverviewComponents.OverviewStats(score, CollectionsService.Label(type).ToLower());
        UiComponents.SeparatorText(CollectionsService.Label(type));
    }

    private bool DrawWarnings() {
        if (type != UnlockableType.Title || Plugin.UnlockState.IsTitleListLoaded) return false;

        var available = ImGui.GetContentRegionAvail();
        var textSize = ImGui.CalcTextSize(TitleListNotLoadedWarning);
        var cursorPos = ImGui.GetCursorPos();

        ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + ((available.X - textSize.X) / 2), Y = cursorPos.Y + ((available.Y - textSize.Y) / 2) });
        ImGui.TextColored(UiColors.Red(), TitleListNotLoadedWarning);
        return true;
    }

    private void DrawMainContent() {
        clipper.Draw(entries.Count, i => {
            UiComponents.CollectionItem(entries[i]);

            if (i != entries.Count - 1) {
                ImGui.Separator();
            }
        });
    }

    public void Draw() {
        var ySize = UiSize.MainContentHeight(plugin.Configuration);
        using var mainContent = ImRaii.Child("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true);
        if (!mainContent) return;

        if (DrawWarnings()) return;

        DrawHeader();
        DrawMainContent();
    }
}
