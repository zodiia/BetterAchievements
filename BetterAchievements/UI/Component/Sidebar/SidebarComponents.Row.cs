using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Component.Sidebar;

public static partial class SidebarComponents {
    private static readonly Vector4 RowTextColor = new(1, 1, 1, 1);
    private const float RowHoverAlpha = 0.08f;
    private const float RowActiveAlpha = 0.16f;
    private const float CategoryIconScale = 0.8f;
    private const float CategoryIconVerticalOffsetEm = 0.15f;
    private const float CategoryRowPaddingEm = 0.25f;

    private static (bool Clicked, Vector2 ContentStart, float ContentWidth) BeginRow(string id, float contentHeight, Vector2 padding, bool selected, float? rightPadding = null) {
        var width = ImGui.GetContentRegionAvail().X;
        var rowHeight = contentHeight + (padding.Y * 2);
        var windowPos = ImGui.GetWindowPos();
        var windowWidth = ImGui.GetWindowSize().X;
        var rowStart = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton(id, new Vector2(width, rowHeight));
        var hovered = ImGui.IsItemHovered();
        var pressed = ImGui.IsItemActive();
        var clicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);

        if (hovered) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        var bgAlpha = pressed || selected ? RowActiveAlpha : hovered ? RowHoverAlpha : 0f;
        if (bgAlpha > 0f) {
            var bgMin = new Vector2(windowPos.X, rowStart.Y);
            var bgMax = new Vector2(windowPos.X + windowWidth, rowStart.Y + rowHeight);
            ImGui.GetWindowDrawList().AddRectFilled(bgMin, bgMax, ImGui.GetColorU32(RowTextColor with { W = bgAlpha }));
        }

        var contentStart = new Vector2(rowStart.X + padding.X, rowStart.Y + padding.Y);
        var contentWidth = width - padding.X - (rightPadding ?? padding.X);
        ImGui.SetCursorScreenPos(new Vector2(rowStart.X, rowStart.Y + rowHeight));

        return (clicked, contentStart, contentWidth);
    }

    private static void RightAlignedText(Vector2 lineStart, float width, Vector4 color, string text) {
        var textSize = ImGui.CalcTextSize(text);
        ImGui.SetCursorScreenPos(new Vector2(lineStart.X + width - textSize.X, lineStart.Y));
        ImGui.TextColored(color, text);
    }

    private static void PaddedProgressBar(string id, float width, float height, float progress, Vector4 color) {
        using var child = ImRaii.Child(id, new Vector2(width, height), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoInputs);
        if (!child) return;

        UiComponents.ProgressBar(progress, color, height: height);
    }

    private static bool CategoryRow(string id, FontAwesomeIcon icon, string name, float progress, Vector4? color, bool active, bool selected) {
        var style = ImGui.GetStyle();
        var barHeight = UiSize.Em(0.2f);
        var lineHeight = ImGui.GetTextLineHeight();
        var contentHeight = lineHeight + style.ItemSpacing.Y + barHeight;
        var padding = new Vector2(UiSize.Em(CategoryRowPaddingEm), UiSize.Em(CategoryRowPaddingEm));
        var iconColumnWidth = lineHeight;

        var (clicked, contentStart, contentWidth) = BeginRow(id, contentHeight, padding, selected);

        using (ImRaii.PushFont(UiBuilder.IconFont)) {
            ImGui.SetWindowFontScale(CategoryIconScale);
            var iconText = icon.ToIconString();
            var iconSize = ImGui.CalcTextSize(iconText);
            var iconX = contentStart.X + ((iconColumnWidth - iconSize.X) / 2f);
            ImGui.SetCursorScreenPos(new Vector2(iconX, contentStart.Y + UiSize.Em(CategoryIconVerticalOffsetEm)));
            if (active) ImGui.TextColored(color ?? UiColors.Orange(), iconText);
            else ImGui.TextUnformatted(iconText);
            ImGui.SetWindowFontScale(1f);
        }

        ImGui.SetCursorScreenPos(contentStart with { X = contentStart.X + iconColumnWidth + style.ItemSpacing.X });
        ImGui.TextColored(RowTextColor, name);

        RightAlignedText(contentStart, contentWidth, UiColors.Grey(), $"{(int)MathF.Round(Math.Clamp(progress, 0f, 1f) * 100)}%");

        ImGui.SetCursorScreenPos(contentStart with { Y = contentStart.Y + lineHeight + style.ItemSpacing.Y });
        PaddedProgressBar($"{id}_bar", contentWidth, barHeight, Math.Clamp(progress, 0f, 1f), color ?? UiColors.Progress());

        return clicked;
    }

    private static bool SubCategoryRow(string id, string name, float progress, bool selected) {
        var style = ImGui.GetStyle();
        var rightPadding = UiSize.Em(CategoryRowPaddingEm);
        var (clicked, contentStart, contentWidth) = BeginRow(id, ImGui.GetTextLineHeight(), new Vector2(0, style.FramePadding.Y), selected, rightPadding);

        ImGui.SetCursorScreenPos(contentStart);
        ImGui.TextColored(RowTextColor, name);
        RightAlignedText(contentStart, contentWidth, UiColors.Grey(), $"{(int)MathF.Round(Math.Clamp(progress, 0f, 1f) * 100)}%");

        return clicked;
    }

    private static void StaticSubCategoryLabel(string name, float progress) {
        var style = ImGui.GetStyle();
        var width = ImGui.GetContentRegionAvail().X - UiSize.Em(CategoryRowPaddingEm);
        var lineStart = ImGui.GetCursorScreenPos();

        ImGui.SetCursorScreenPos(lineStart with { Y = lineStart.Y + style.FramePadding.Y });
        ImGui.TextColored(RowTextColor, name);
        RightAlignedText(lineStart with { Y = lineStart.Y + style.FramePadding.Y }, width, UiColors.Grey(), $"{(int)MathF.Round(Math.Clamp(progress, 0f, 1f) * 100)}%");

        var rowHeight = ImGui.GetTextLineHeight() + (style.FramePadding.Y * 2);
        ImGui.SetCursorScreenPos(lineStart with { Y = lineStart.Y + rowHeight });
    }
}
