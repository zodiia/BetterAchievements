using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents {
    private static readonly Dictionary<uint, float> CalloutHeights = new();

    public static void Callout(Vector4 color, Action content) {
        using var borderColor = ImRaii.PushColor(ImGuiCol.Border, color);
        using var backgroundColor = ImRaii.PushColor(ImGuiCol.ChildBg, color with { W = 0.1f });

        var id = ImGui.GetID("Callout");
        var width = ImGui.GetContentRegionAvail().X;
        var height = CalloutHeights.GetValueOrDefault(id, ImGui.GetTextLineHeightWithSpacing() * 4f);

        using var child = ImRaii.Child(
            "Callout",
            new Vector2(width, height),
            true,
            ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        if (!child) return;

        var contentStart = ImGui.GetCursorPosY();
        content();
        var contentEnd = ImGui.GetCursorPosY();

        CalloutHeights[id] = contentEnd - contentStart + (ImGui.GetStyle().WindowPadding.Y * 2);
    }
}
