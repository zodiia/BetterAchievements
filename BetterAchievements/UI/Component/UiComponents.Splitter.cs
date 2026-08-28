using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents {
    public static void VerticalSplitter(string id, ref float leftWidth, float min, float max, float height, float thickness = 8f) {
        ImGui.SameLine(0, 0);

        var cursor = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton(id, new Vector2(thickness, height));

        var hovered = ImGui.IsItemHovered();
        var active = ImGui.IsItemActive();

        if (hovered || active) {
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
            var lineX = cursor.X + (thickness / 2);
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(lineX, cursor.Y),
                new Vector2(lineX, cursor.Y + height),
                ImGui.GetColorU32(active ? ImGuiCol.SeparatorActive : ImGuiCol.SeparatorHovered),
                2f
            );
        }

        if (active) {
            leftWidth = Math.Clamp(leftWidth + ImGui.GetIO().MouseDelta.X, min, max);
        }

        ImGui.SameLine(0, 0);
    }
}
