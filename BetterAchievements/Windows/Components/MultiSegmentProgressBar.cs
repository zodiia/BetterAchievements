using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Serilog;

namespace BetterAchievements.Windows.Components;

public struct BarSegment
{
    public float Proportion;
    public Vector4 Color;
    public string Tooltip;
}

public static partial class ImGuiComponents
{
    private const float SegmentSpacingPx = 2;

    private static float MultiSegmentProgressBarSegment(BarSegment segment, float width, float currentX, float progressBoundaryX, Vector2 barStart, Vector2 barEnd)
    {
        var drawList = ImGui.GetWindowDrawList();
        float segWidth = width * segment.Proportion - SegmentSpacingPx;
        Vector2 segStart = barStart with { X = currentX };
        Vector2 segEnd = barEnd with { X = currentX + segWidth };

        if (segWidth <= 0)
        {
            return currentX;
        }
        if (currentX < progressBoundaryX)
        {
            var drawWidth = segWidth;
            if (currentX + segWidth > progressBoundaryX)
            {
                drawWidth = progressBoundaryX - currentX;
            }
            Vector2 drawEnd = barEnd with { X = currentX + drawWidth };
            drawList.AddRectFilled(segStart, drawEnd, ImGui.GetColorU32(segment.Color), 4.0f);
        }

        if (ImGui.IsMouseHoveringRect(segStart, segEnd))
        {
            ImGui.SetTooltip(segment.Tooltip);
        }

        return currentX + segWidth + SegmentSpacingPx;
    }

    public static void MultiSegmentProgressBar(IEnumerable<BarSegment> segments, float progress, float height = 25f)
    {
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X + SegmentSpacingPx;
        var clampedProgress = Math.Clamp(progress, 0f, 1f);
        var progressBoundaryX = cursorPos.X + (width * clampedProgress);

        // Background
        Vector2 barStart = cursorPos;
        Vector2 barEnd = new Vector2(barStart.X + width, barStart.Y + height);
        drawList.AddRectFilled(barStart, barEnd, ImGui.GetColorU32(ImGuiCol.FrameBg), 4.0f);

        float currentX = barStart.X;
        foreach (var segment in segments)
        {
            currentX = MultiSegmentProgressBarSegment(segment, width, currentX, progressBoundaryX, barStart, barEnd);
        }
        ImGui.Dummy(new Vector2(width, height));
    }
}
