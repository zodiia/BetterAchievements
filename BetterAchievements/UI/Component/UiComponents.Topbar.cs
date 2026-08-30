using System.Numerics;
using BetterAchievements.UI.State;
using BetterAchievements.UI.Windows;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents {
    public static void Topbar(Plugin plugin, MainWindowState state) {
        var cellPadding = ImGui.GetStyle().CellPadding;   // outside search bar
        var framePadding = ImGui.GetStyle().FramePadding; // inside search bar
        var margin = ImGui.GetStyle().WindowPadding;
        var childHeight = ImGui.GetTextLineHeight() + (cellPadding.Y * 2) + (framePadding.Y * 2) + (margin.Y * 2);
        using var child = ImRaii.Child("TopbarLayout##Topbar", ImGui.GetContentRegionAvail() with { Y = childHeight }, true, ImGuiWindowFlags.AlwaysAutoResize);
        if (!child.Success) return;

        var startingY = ImGui.GetCursorPosY();

        ImGui.SetCursorPosY(startingY + cellPadding.Y + framePadding.Y);
        ImGui.Text("Search:");

        ImGui.SameLine();
        ImGui.SetCursorPosY(startingY + cellPadding.Y);
        if (ImGui.InputTextEx("", "Search achievements", ref state.SearchBuffer, 128, default(Vector2) with { X = 400 })) {
            state.SetSearch(state.SearchBuffer); // do not recalculate ToLower many times per frames
        }

        ImGui.SameLine();
        ImGui.SetCursorPosY(startingY + cellPadding.Y);
        if (ImGuiComponents.IconButton(FontAwesomeIcon.SlidersH)) {
            ImGui.OpenPopup(ConfigPopup.FiltersPopupId);
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Settings");
        }

        ImGui.SameLine();
        ImGui.SetCursorPosY(startingY + cellPadding.Y);
        if (ImGuiComponents.IconButton(FontAwesomeIcon.SyncAlt)) {
            state.Refresh();
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Refresh UI state");
        }

        ConfigPopup.FiltersPopup(plugin, state);

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        var icon = FontAwesomeIcon.Trophy.ToIconString();
        var iconSize = ImGui.CalcTextSize(icon);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - iconSize.X);
        ImGui.SetCursorPosY(startingY + cellPadding.Y - 1);
        ImGui.TextColored(UiColors.Orange(), icon);
        ImGui.PopFont();

        ImGui.SameLine();
        var achievementPointsText = $"{state.Unlockables.AchievementPoints.Obtained} ";
        var achievementPointsSize = ImGui.CalcTextSize(achievementPointsText);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - (iconSize.X + achievementPointsSize.X));
        ImGui.SetCursorPosY(startingY + cellPadding.Y);
        ImGui.TextColored(UiColors.Orange(), achievementPointsText);
    }
}
