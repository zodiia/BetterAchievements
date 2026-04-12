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
        Vector2 barStart = cursorPos;
        Vector2 barEnd = new Vector2(barStart.X + width, barStart.Y + height);
        drawList.AddRectFilled(barStart, barEnd, ImGui.GetColorU32(ImGuiCol.FrameBg), 4.0f);

        // Tooltip & Click handling
        if (ImGui.IsMouseHoveringRect(barStart, barEnd))
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
            Vector2 textPosition = new() { X = barStart.X + ((width - insideTextSize.X) / 2), Y = barStart.Y + ((height - insideTextSize.Y) / 2) };
            ImGui.SetCursorScreenPos(textPosition);
            ImGui.TextColored(color, insideText ?? "Disabled");
            return;
        }

        // Filled
        Vector2 fillEnd = new Vector2(barStart.X + (width * clampedProgress), barStart.Y + height);
        drawList.AddRectFilled(barStart, fillEnd, ImGui.GetColorU32(color), 4.0f);

        // Inside Text
        if (insideText != null)
        {
            if (fillEnd.X + UiSize.Em(0.5f) + insideTextSize.X < barEnd.X - UiSize.Em(0.5f))
            {
                ImGui.SetCursorScreenPos(new() { X = fillEnd.X + UiSize.Em(0.5f), Y = barStart.Y + ((height - insideTextSize.Y) / 2) });
                ImGui.TextColored(color, insideText);
            }
            else
            {
                ImGui.SetCursorScreenPos(new() { X = fillEnd.X - UiSize.Em(0.5f) - insideTextSize.X, Y = barStart.Y + ((height - insideTextSize.Y) / 2) });
                ImGui.TextColored(UiColors.ColorBlack(), insideText);
            }
        }

        ImGui.SetCursorScreenPos(barStart);
        ImGui.Dummy(new Vector2(width, height));
    }
}
