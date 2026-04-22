using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents
{
    public static void ProgressBar(
        float progress,
        Vector4 color,
        string? tooltip = null,
        string? insideText = null,
        float height = 25f,
        bool enabled = true,
        Action? onClick = null)
    {
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var clampedProgress = Math.Clamp(progress, 0f, 1f);
        var insideTextSize = insideText != null ? ImGui.CalcTextSize(insideText) : Vector2.Zero;

        // Background
        var barEnd = new Vector2(cursorPos.X + width, cursorPos.Y + height);
        drawList.AddRectFilled(cursorPos, barEnd, ImGui.GetColorU32(ImGuiCol.FrameBg), 4.0f);

        // Tooltip & Click handling
        if (ImGui.IsMouseHoveringRect(cursorPos, barEnd))
        {
            if (tooltip != null)
            {
                ImGui.SetTooltip(tooltip);
            }

            if (onClick != null)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left, false))
            {
                onClick?.Invoke();
            }
        }

        // Disabled bar
        if (!enabled)
        {
            Vector2 textPosition = new() { X = cursorPos.X + ((width - insideTextSize.X) / 2), Y = cursorPos.Y + ((height - insideTextSize.Y) / 2) };
            ImGui.SetCursorScreenPos(textPosition);
            ImGui.TextColored(color, insideText ?? "Disabled");

            ImGui.SetCursorScreenPos(cursorPos);
            ImGui.Dummy(new Vector2(width, height));
            return;
        }

        // Filled
        Vector2 fillEnd = new Vector2(cursorPos.X + (width * clampedProgress), cursorPos.Y + height);
        drawList.AddRectFilled(cursorPos, fillEnd, ImGui.GetColorU32(color), 4.0f);

        // Inside Text
        if (insideText != null)
        {
            if (fillEnd.X + UiSize.Em(0.5f) + insideTextSize.X < barEnd.X - UiSize.Em(0.5f))
            {
                ImGui.SetCursorScreenPos(new() { X = fillEnd.X + UiSize.Em(0.5f), Y = cursorPos.Y + ((height - insideTextSize.Y) / 2) });
                ImGui.TextColored(color, insideText);
            }
            else
            {
                ImGui.SetCursorScreenPos(new() { X = fillEnd.X - UiSize.Em(0.5f) - insideTextSize.X, Y = cursorPos.Y + ((height - insideTextSize.Y) / 2) });
                ImGui.TextColored(UiColors.Black(), insideText);
            }
        }

        ImGui.SetCursorScreenPos(cursorPos);
        ImGui.Dummy(new Vector2(width, height));
    }
}
