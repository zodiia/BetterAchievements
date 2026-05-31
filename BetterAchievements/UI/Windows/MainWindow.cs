using System;
using System.Linq;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.Component;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace BetterAchievements.UI.Windows;

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
        state = new MainWindowState(plugin);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(900, 450),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    private void DrawTopbarLayout()
    {
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
        if (ImGui.InputTextEx("", "Search achievements", ref state.SearchBuffer, 128, default(Vector2) with { X = 400 }))
        {
            state.SetSearch(state.SearchBuffer); // do not recalculate ToLower many times per frames
        }

        ImGui.SameLine();
        ImGui.SetCursorPosY(startingY + cellPadding.Y);
        if (ImGuiComponents.IconButton(FontAwesomeIcon.SlidersH))
        {
            ImGui.OpenPopup(FilterPopup.FiltersPopupId);
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Filters and sorting options");
        }

        ImGui.SameLine();
        ImGui.SetCursorPosY(startingY + cellPadding.Y);
        if (ImGuiComponents.IconButton(FontAwesomeIcon.SyncAlt))
        {
            state.Refresh();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Refresh UI state");
        }

        FilterPopup.FiltersPopup(state);

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        var icon = FontAwesomeIcon.Trophy.ToIconString();
        var iconSize = ImGui.CalcTextSize(icon);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - iconSize.X);
        ImGui.SetCursorPosY(startingY + cellPadding.Y - 1);
        ImGui.TextColored(UiColors.Orange(), icon);
        ImGui.PopFont();

        ImGui.SameLine();
        var achievementPointsText = $"{state.AchievementPoints} ";
        var achievementPointsSize = ImGui.CalcTextSize(achievementPointsText);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - (iconSize.X + achievementPointsSize.X));
        ImGui.SetCursorPosY(startingY + cellPadding.Y);
        ImGui.TextColored(UiColors.Orange(), achievementPointsText);
    }

    private bool DrawWarnings()
    {
        if (!Plugin.UnlockState.IsAchievementListLoaded)
        {
            var available = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(AchievementListNotLoadedWarning);
            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
            ImGui.TextColored(UiColors.Red(), AchievementListNotLoadedWarning);
            return true;
        }

        if (state.SelectedCategoryId == MainWindowState.NoCategoryId)
        {
            var available = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(NoCategorySelectedWarning);
            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new() { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
            ImGui.TextColored(UiColors.Red(), NoCategorySelectedWarning);
            return true;
        }

        return false;
    }

    private void DrawAchievementsMainContent()
    {
        foreach (var it in state.CategoryUnlockables)
        {
            if (it is UnlockableAchievement achievement)
            {
                if (achievement.Maximum() > 1)
                {
                    UiComponents.ProgressBasedAchievement(achievement, state, plugin);
                }
                else
                {
                    UiComponents.SimpleAchievement(achievement, state, plugin);
                }
            }

            if (it is UnlockableTieredAchievement tiered)
            {
                UiComponents.TieredAchievement(tiered, state, plugin);
            }

            if (it != state.CategoryUnlockables.Last())
            {
                ImGui.Separator();
            }
        }
    }

    private void DrawMainContent()
    {
        if (!ImGui.BeginChild("MainContent", ImGui.GetContentRegionAvail(), true))
        {
            return;
        }

        if (DrawWarnings() || state.SelectedAchievementCategory == null) // null is already checked but just doing that so that my ide stops screaming at me
        {
            ImGui.EndChild();
            return;
        }

        DrawAchievementsMainContent();

        ImGui.EndChild();
    }

    private void DrawPinnedAchievementsSidebarItem()
    {
        if (ImGui.TreeNodeEx("Pinned", ImGuiTreeNodeFlags.Leaf |
                                       ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                       ImGuiTreeNodeFlags.SpanFullWidth |
                                       (state.SelectedCategoryId == MainWindowState.PinnedAchievementsCategoryId ? ImGuiTreeNodeFlags.Selected : 0)))
        {
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                state.SetCategory(MainWindowState.PinnedAchievementsCategoryId);
            }
        }
    }

    private void DrawSidebarItem(string name, int categoryId)
    {
        if (ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.Leaf |
                                   ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                   ImGuiTreeNodeFlags.SpanFullWidth |
                                   (state.SelectedCategoryId == categoryId ? ImGuiTreeNodeFlags.Selected : 0)))
        {
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                state.SetCategory(categoryId);
            }
        }
    }

    private void DrawSidebarAchievementLayout(AchievementLayout layout)
    {
        switch (layout)
        {
            case AchievementLayoutGroup group:
                if (ImGui.TreeNodeEx(group.Name, ImGuiTreeNodeFlags.SpanFullWidth))
                {
                    foreach (var subLayout in group.Items)
                    {
                        DrawSidebarAchievementLayout(subLayout);
                    }
                    ImGui.TreePop();
                }

                break;

            case AchievementLayoutCategory category:
                DrawSidebarItem(category.Name, category.Id);

                break;
        }
    }

    private void DrawSidebar()
    {
        using var sidebar = ImRaii.Child("Sidebar", ImGui.GetContentRegionAvail() with { X = 350 }, true);
        if (!sidebar) return;

        if (ImGui.CollapsingHeader("Achievements"))
        {
            DrawPinnedAchievementsSidebarItem();

            foreach (var layout in state.FilteredLayout.AchievementLayout)
            {
                DrawSidebarAchievementLayout(layout);
            }
        }

        if (ImGui.CollapsingHeader("Fishing"))
        {
            ImGui.TreeNodeEx("Fish Guide", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Spearfish Guide", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            using var regions = ImRaii.TreeNode("Regions", ImGuiTreeNodeFlags.SpanFullWidth);
            if (regions.Success)
            {
                using var region = ImRaii.TreeNode("La Noscea", ImGuiTreeNodeFlags.SpanFullWidth);
                if (region.Success)
                {
                    using var area = ImRaii.TreeNode("Middle La Noscea", ImGuiTreeNodeFlags.SpanFullWidth);
                    if (area.Success)
                    {
                        ImGui.TreeNodeEx("Zephyr Drift", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("Summerfold", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("Rogue River", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("West Agelyss River", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("Nym River", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                        ImGui.TreeNodeEx("Woad Whisper Canyon", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);

                    }
                }
            }
        }

        if (ImGui.CollapsingHeader("Collectibles"))
        {
            ImGui.TreeNodeEx("Mounts", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Minions", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Titles", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Triple Triad Cards", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Triple Triad NPCs", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Orchestrion Rolls", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Portraits", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Levequests", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Bardings", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Emotes", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TreeNodeEx("Fashion Accessories", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
        }
    }

    public override void Draw()
    {
        state.CheckForUiRefresh();
        DrawTopbarLayout();
        DrawSidebar();
        ImGui.SameLine();
        DrawMainContent();
    }
}
