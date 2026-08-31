using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents {
    public static void ProgressBar(
        float progress,
        Vector4 color,
        string? tooltip = null,
        string? insideText = null,
        float height = 25f,
        float? width = null,
        bool enabled = true,
        Action? onClick = null,
        bool? border = null
    ) {
        var style = ImGui.GetStyle();
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var barWidth = width ?? ImGui.GetContentRegionAvail().X;
        var clampedProgress = Math.Clamp(progress, 0f, 1f);
        var insideTextSize = insideText != null ? ImGui.CalcTextSize(insideText) : Vector2.Zero;
        var drawBorder = border ?? style.FrameBorderSize > 0f;
        var borderThickness = style.FrameBorderSize > 0f ? style.FrameBorderSize : 1f;

        // Background
        var barEnd = new Vector2(cursorPos.X + barWidth, cursorPos.Y + height);
        drawList.AddRectFilled(cursorPos, barEnd, ImGui.GetColorU32(ImGuiCol.FrameBg), style.FrameRounding);

        // Tooltip & Click handling
        if (ImGui.IsMouseHoveringRect(cursorPos, barEnd)) {
            if (tooltip != null) {
                ImGui.SetTooltip(tooltip);
            }

            if (onClick != null) {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left, false)) {
                onClick?.Invoke();
            }
        }

        // Disabled bar
        if (!enabled) {
            Vector2 textPosition = new() { X = cursorPos.X + ((barWidth - insideTextSize.X) / 2), Y = cursorPos.Y + ((height - insideTextSize.Y) / 2) };
            ImGui.SetCursorScreenPos(textPosition);
            ImGui.TextColored(color, insideText ?? "Disabled");

            if (drawBorder) {
                drawList.AddRect(cursorPos, barEnd, ImGui.GetColorU32(ImGuiCol.Border), style.FrameRounding, borderThickness);
            }

            ImGui.SetCursorScreenPos(cursorPos);
            ImGui.Dummy(new Vector2(barWidth, height));
            if (onClick == null) ImGui.SetItemAllowOverlap();
            return;
        }

        // Filled
        Vector2 fillEnd = new Vector2(cursorPos.X + (barWidth * clampedProgress), cursorPos.Y + height);
        drawList.AddRectFilled(cursorPos, fillEnd, ImGui.GetColorU32(color), style.FrameRounding);

        // Inside Text
        if (insideText != null) {
            if (fillEnd.X + UiSize.Em(0.5f) + insideTextSize.X < barEnd.X - UiSize.Em(0.5f)) {
                ImGui.SetCursorScreenPos(new() { X = fillEnd.X + UiSize.Em(0.5f), Y = cursorPos.Y + ((height - insideTextSize.Y) / 2) });
                ImGui.TextColored(color, insideText);
            } else {
                ImGui.SetCursorScreenPos(new() { X = fillEnd.X - UiSize.Em(0.5f) - insideTextSize.X, Y = cursorPos.Y + ((height - insideTextSize.Y) / 2) });
                ImGui.TextColored(UiColors.Black(), insideText);
            }
        }

        // Border
        if (drawBorder) {
            drawList.AddRect(cursorPos, barEnd, ImGui.GetColorU32(ImGuiCol.Border), style.FrameRounding, borderThickness);
        }

        ImGui.SetCursorScreenPos(cursorPos);
        ImGui.Dummy(new Vector2(barWidth, height));
        if (onClick == null) ImGui.SetItemAllowOverlap();
    }
}
