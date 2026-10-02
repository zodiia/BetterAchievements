using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.Data;
using Recollection.Data.Unlockable;
using Recollection.UI.Component;
using Recollection.UI.State;

namespace Recollection.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    private const int CategoryColumns = 3;

    public static void CategoriesGrid(
        Plugin plugin, UnlockablesState unlockables, NavigationState navigation, IEnumerable<AchievementLayout>? layouts = null, int columns = CategoryColumns
    ) {
        if (!ImGui.BeginTable("CategoriesGrid", columns, ImGuiTableFlags.SizingStretchSame)) return;

        foreach (var layout in layouts ?? unlockables.FilteredLayout.Achievements) {
            ImGui.TableNextColumn();
            CategoryCard(plugin, unlockables, navigation, layout);
        }

        ImGui.EndTable();
    }

    private static void NavigateToCategory(NavigationState navigation, AchievementLayout layout) {
        switch (layout) {
            case AchievementLayoutGroup group:
                navigation.Navigate(new NavigationTarget.Group(group.Name));
                break;
            case AchievementLayoutCategory category:
                navigation.Navigate(new NavigationTarget.Category(category.Id));
                break;
        }
    }

    public static void CollectionCategoriesGrid(
        Plugin plugin, UnlockablesState unlockables, NavigationState navigation, UnlockableType type, int columns = CategoryColumns) {
        using var table = ImRaii.Table("CollectionCategoriesGrid", columns, ImGuiTableFlags.SizingStretchSame);
        if (!table) return;

        foreach (var category in unlockables.CollectionCategories(type)) {
            var (score, visible) = unlockables.ComputeProgress(type, category);
            if (visible == 0) continue;

            ImGui.TableNextColumn();
            CategoryCard(plugin, $"##CollectionCategory-{type}-{category.Id}", category.Name, score, null,
                         () => navigation.Navigate(new NavigationTarget.CollectionCategory(type, category.Id)));
        }
    }

    private static void CategoryCard(Plugin plugin, UnlockablesState unlockables, NavigationState navigation, AchievementLayout layout) {
        CategoryCard(plugin, $"##Category-{layout.Name}", layout.Name,
                     unlockables.ComputeAchievementCount(layout), unlockables.ComputeProgress(layout),
                     () => NavigateToCategory(navigation, layout));
    }

    private static void CategoryCard(Plugin plugin, string id, string name, PointsScore count, PointsScore? points, Action onClick) {
        var padding = UiSize.Em(0.5f) * plugin.Configuration.UiDensity;
        var progress = ProgressRatio(count, points);
        var width = ImGui.GetContentRegionAvail().X;
        var rowStart = ImGui.GetCursorScreenPos();
        var leftPadding = ImGui.TableGetColumnIndex() % ImGui.TableGetColumnCount() == 0 ? 0 : padding;
        var rightPadding = ImGui.TableGetColumnIndex() % ImGui.TableGetColumnCount() == ImGui.TableGetColumnCount() - 1 ? 0 : padding;
        var contentStart = new Vector2(rowStart.X + leftPadding, rowStart.Y + padding);
        var contentWidth = width - leftPadding - rightPadding;
        var thirdLineStart = contentStart with { Y = contentStart.Y + ImGui.GetTextLineHeight() + UiSize.Em(0.5f) + (ImGui.GetStyle().ItemSpacing.Y * 2) };

        var clicked = CardBackground(id, rowStart, width, padding);
        CardTitle(contentStart, name);
        CardPercentage(contentStart, contentWidth, progress);
        CardProgressBar(contentStart, contentWidth, progress);
        CardCount(thirdLineStart, count);
        CardPoints(thirdLineStart, contentWidth, points);

        if (clicked) onClick();
    }

    private static bool CardBackground(string id, Vector2 start, float width, float padding) {
        var height = (ImGui.GetTextLineHeight() * 2) + UiSize.Em(0.5f) + (ImGui.GetStyle().ItemSpacing.Y * 2) + (padding * 2);
        var size = new Vector2(width, height);

        ImGui.InvisibleButton(id, size);
        if (ImGui.IsItemHovered()) {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            ImGui.GetWindowDrawList().AddRectFilled(start, start + size, ImGui.GetColorU32(UiColors.Text() with { W = 0.06f }), ImGui.GetStyle().FrameRounding);
        }

        return ImGui.IsItemClicked(ImGuiMouseButton.Left);
    }

    private static void CardTitle(Vector2 contentStart, string name) {
        ImGui.SetCursorScreenPos(contentStart);
        ImGui.TextColored(UiColors.Text(), name);
    }

    private static void CardPercentage(Vector2 contentStart, float contentWidth, float progress) {
        RightAlignedText(contentStart, contentWidth, UiColors.Grey(), $"{(int)MathF.Round(progress * 100)}%");
    }

    private static void CardProgressBar(Vector2 contentStart, float contentWidth, float progress) {
        ImGui.SetCursorScreenPos(contentStart with { Y = contentStart.Y + ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y });
        UiComponents.ProgressBar(progress, UiColors.Progress(), height: UiSize.Em(0.5f), width: contentWidth); // todo: progress bar height currently not decided by settings
    }

    private static void CardCount(Vector2 lineStart, PointsScore count) {
        var (obtainedCount, totalCount) = count;
        ImGui.SetCursorScreenPos(lineStart);
        ImGui.TextColored(UiColors.Blue(), $"{obtainedCount:N0}");
        ImGui.SameLine(0, 0);
        ImGui.TextColored(UiColors.Text(), $"/{totalCount:N0}");
    }

    private static void CardPoints(Vector2 lineStart, float contentWidth, PointsScore? points) {
        if (points == null) return;
        RightAlignedSplitText(lineStart, contentWidth, UiColors.Progress(), $"{points.Obtained:N0}", UiColors.Text(), $"/{points.Total:N0}");
    }

    private static void RightAlignedSplitText(Vector2 lineStart, float width, Vector4 firstColor, string first, Vector4 restColor, string rest) {
        var totalWidth = ImGui.CalcTextSize(first).X + ImGui.CalcTextSize(rest).X;
        ImGui.SetCursorScreenPos(new Vector2(lineStart.X + width - totalWidth, lineStart.Y));
        ImGui.TextColored(firstColor, first);
        ImGui.SameLine(0, 0);
        ImGui.TextColored(restColor, rest);
    }
}
