using System;
using System.Collections.Generic;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    private const int CategoryColumns = 3;

    public static void CategoriesGrid(UnlockablesState unlockables, NavigationState navigation, IEnumerable<AchievementLayout>? layouts = null, int columns = CategoryColumns) {
        if (!ImGui.BeginTable("CategoriesGrid", columns, ImGuiTableFlags.SizingStretchSame)) return;

        foreach (var layout in layouts ?? unlockables.FilteredLayout.AchievementLayout) {
            ImGui.TableNextColumn();
            CategoryCard(unlockables, navigation, layout);
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

    private static void CategoryCard(UnlockablesState unlockables, NavigationState navigation, AchievementLayout layout) {
        var (obtainedCount, totalCount) = unlockables.ComputeAchievementCount(layout);
        var (obtainedPoints, totalPoints) = unlockables.ComputeProgress(layout);
        var progress = totalPoints == 0 ? 0f : (float)obtainedPoints / totalPoints;

        var style = ImGui.GetStyle();
        var barHeight = UiSize.Em(0.5f);
        var lineHeight = ImGui.GetTextLineHeight();
        var contentHeight = (lineHeight * 2) + barHeight + (style.ItemSpacing.Y * 2);
        var padding = new Vector2(UiSize.Em(0.5f), UiSize.Em(0.5f));
        var width = ImGui.GetContentRegionAvail().X;
        var rowHeight = contentHeight + (padding.Y * 2);

        var rowStart = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton($"##Category-{layout.Name}", new Vector2(width, rowHeight));
        var hovered = ImGui.IsItemHovered();
        var clicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
        if (hovered) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        if (hovered) {
            var bgMax = new Vector2(rowStart.X + width, rowStart.Y + rowHeight);
            ImGui.GetWindowDrawList().AddRectFilled(rowStart, bgMax, ImGui.GetColorU32(UiColors.Text() with { W = 0.06f }), style.FrameRounding);
        }

        var contentStart = new Vector2(rowStart.X + padding.X, rowStart.Y + padding.Y);
        var contentWidth = width - (padding.X * 2);

        ImGui.SetCursorScreenPos(contentStart);
        ImGui.TextColored(UiColors.Text(), layout.Name);
        RightAlignedText(contentStart, contentWidth, UiColors.Grey(), $"{(int)MathF.Round(progress * 100)}%");

        var barPos = contentStart with { Y = contentStart.Y + lineHeight + style.ItemSpacing.Y };
        ImGui.SetCursorScreenPos(barPos);
        UiComponents.ProgressBar(progress, UiColors.Progress(), height: barHeight, width: contentWidth);

        var line3Pos = contentStart with { Y = barPos.Y + barHeight + style.ItemSpacing.Y };
        ImGui.SetCursorScreenPos(line3Pos);
        ImGui.TextColored(UiColors.Blue(), $"{obtainedCount:N0}");
        ImGui.SameLine(0, 0);
        ImGui.TextColored(UiColors.Text(), $"/{totalCount:N0}");

        RightAlignedSplitText(line3Pos, contentWidth, UiColors.Progress(), $"{obtainedPoints:N0}", UiColors.Text(), $"/{totalPoints:N0}");

        if (clicked) {
            NavigateToCategory(navigation, layout);
        }
    }

    private static void RightAlignedSplitText(Vector2 lineStart, float width, Vector4 firstColor, string first, Vector4 restColor, string rest) {
        var totalWidth = ImGui.CalcTextSize(first).X + ImGui.CalcTextSize(rest).X;
        ImGui.SetCursorScreenPos(new Vector2(lineStart.X + width - totalWidth, lineStart.Y));
        ImGui.TextColored(firstColor, first);
        ImGui.SameLine(0, 0);
        ImGui.TextColored(restColor, rest);
    }
}
