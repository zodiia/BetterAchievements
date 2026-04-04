using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using BetterAchievements.Unlockables;
using BetterAchievements.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiComponentsDalamud = Dalamud.Interface.Components.ImGuiComponents;
using Serilog;

namespace BetterAchievements.Windows;

public class MainWindow : Window, IDisposable
{
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";
    private const string NoCategorySelectedWarning = "Please select a category.";

    private readonly Plugin plugin;

    private readonly MainWindowState state;

    public MainWindow(Plugin plugin)
        : base("Better Achievements")
    {
        this.plugin = plugin;
        state = new(plugin.MainWindowLayout);
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(900, 450),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    private void DrawTopbarLayout()
    {
        if (ImGui.BeginChild("TopbarLayout", ImGui.GetContentRegionAvail() with { Y = 48 }, true))
        {
            var padding = ImGui.GetStyle().CellPadding;

            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + padding.Y + 1);
            ImGui.Text("Search:");

            ImGui.SameLine();
            if (ImGui.InputTextEx("", "Search achievements", ref state.SearchBuffer, 128, default(Vector2) with { X = 400 }))
            {
                state.SetSearch(state.SearchBuffer); // do not recalculate ToLower many times per frames
            }

            ImGui.SameLine();
            if (ImGuiComponentsDalamud.IconButton(FontAwesomeIcon.SlidersH))
            {
                ImGui.OpenPopup(ImGuiComponents.FiltersPopupId);
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Filters and sorting options");
            }

            ImGuiComponents.FiltersPopup(state);

            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            var icon = FontAwesomeIcon.Trophy.ToIconString();
            var iconSize = ImGui.CalcTextSize(icon);
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - iconSize.X);
            ImGui.SetCursorPosY((ImGui.GetContentRegionAvail().Y - iconSize.Y) / 2);
            ImGui.TextColored(ImGuiComponents.ColorOrange(), icon);
            ImGui.PopFont();

            ImGui.SameLine();
            var achievementPointsText = $"12345 ";
            var achievementPointsSize = ImGui.CalcTextSize(achievementPointsText);
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - (iconSize.X + achievementPointsSize.X));
            ImGui.SetCursorPosY((ImGui.GetContentRegionAvail().Y - achievementPointsSize.Y) / 2);
            ImGui.TextColored(ImGuiComponents.ColorOrange(), achievementPointsText);
            ImGui.EndChild();
        }
    }

    private bool DrawWarnings()
    {
        if (!Plugin.UnlockState.IsAchievementListLoaded)
        {
            var available = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(AchievementListNotLoadedWarning);
            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new() { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
            ImGui.TextColored(ImGuiComponents.ColorRed(), AchievementListNotLoadedWarning);
            return true;
        }

        if (state.SelectedCategory == null)
        {
            var available = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(NoCategorySelectedWarning);
            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new() { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
            ImGui.TextColored(ImGuiComponents.ColorRed(), NoCategorySelectedWarning);
            return true;
        }

        return false;
    }

    private void DrawMainContent()
    {
        if (!ImGui.BeginChild("MainContent", ImGui.GetContentRegionAvail(), true))
        {
            return;
        }

        if (DrawWarnings() || state.SelectedCategory == null) // null is already checked but just doing that so that my ide stops screaming at me
        {
            ImGui.EndChild();
            return;
        }

        foreach (var it in state.CategoryUnlockables)
        {
            if (it is UnlockableAchievement achievement)
            {
                if (achievement.Maximum() != null && achievement.Maximum() > 1)
                {
                    ImGuiComponents.ProgressBasedAchievement(achievement);
                }
                else
                {
                    ImGuiComponents.SimpleAchievement(achievement);
                }
            }

            if (it is UnlockableTieredAchievement tiered)
            {
                ImGuiComponents.MultiProgressBasedAchievement(tiered);
            }

            if (it != state.CategoryUnlockables.Last())
            {
                ImGui.Separator();
            }
        }

        ImGui.EndChild();
    }

    private void DrawSidebarLayout(AchievementLayout layout)
    {
        if (layout is AchievementLayoutGroup group)
        {
            if (ImGui.TreeNodeEx(group.Name, ImGuiTreeNodeFlags.SpanFullWidth))
            {
                foreach (var subLayout in group.Items)
                {
                    DrawSidebarLayout(subLayout);
                }
                ImGui.TreePop();
            }
        }
        else if (layout is AchievementLayoutCategory category)
        {
            if (ImGui.TreeNodeEx(category.Name, ImGuiTreeNodeFlags.Leaf |
                                                ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                                ImGuiTreeNodeFlags.SpanFullWidth |
                                                (state.SelectedCategory?.Id == category.Id ? ImGuiTreeNodeFlags.Selected : 0)))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    state.SetCategory(category.Id);
                }
            }
        }
    }

    private void DrawSidebar()
    {
        if (ImGui.BeginChild("Sidebar", ImGui.GetContentRegionAvail() with { X = 350 }, true))
        {
            if (!ImGui.CollapsingHeader("Achievements"))
            {
                ImGui.EndChild();
                return;
            }

            foreach (var layout in state.FilteredLayout.AchievementLayout)
            {
                DrawSidebarLayout(layout);
            }
            ImGui.EndChild();
        }
    }

    public override void Draw()
    {
        DrawTopbarLayout();
        DrawSidebar();
        ImGui.SameLine();
        DrawMainContent();
    }
}
