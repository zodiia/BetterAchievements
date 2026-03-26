using System;
using System.Linq;
using System.Numerics;
using BetterAchievements.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;

namespace BetterAchievements.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private AchievementLayoutCategory? selectedLayoutCategory;

    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";
    private const string NoCategorySelectedWarning = "Please select a category.";

    private string searchBuffer = "";
    private string searchTerm = "";

    public MainWindow(Plugin plugin)
        : base("Better Achievements")
    {
        this.plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
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
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + padding.Y);
            ImGui.Text("Search:");
            ImGui.SameLine();
            if (ImGui.InputTextEx("", "Search achievements", ref searchBuffer, 128, default(Vector2) with { X = 400 }))
            {
                searchTerm = searchBuffer.ToLower(); // do not recalculate ToLower many times per frames
            }

            ImGui.SameLine();

            var achievementPointsText = $"9945 {FontAwesomeIcon.Trophy.ToIconString()}";
            var size = ImGui.CalcTextSize(achievementPointsText);

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - size.X);
            ImGui.PushFont(ImGui.GetFont() with { Scale = 1.5f });
            ImGui.TextColored(ImGuiComponents.ColorOrange(), achievementPointsText);
            ImGui.PopFont();

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

        if (selectedLayoutCategory == null)
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

        if (DrawWarnings() || selectedLayoutCategory == null) // null is already checked but just doing that so that my ide stops screaming at me
        {
            ImGui.EndChild();
            return;
        }

        foreach (var achievement in selectedLayoutCategory.Items)
        {
            if (achievement is AchievementLayoutItemSimple simple)
            {
                var unlockable = Plugin.UnlockablesService.GetUnlockableAchievement(simple.Id);
                if (searchBuffer != "" && !unlockable.NameLowercase.Contains(searchTerm))
                {
                    continue;
                }
                if (unlockable.Maximum() != null && unlockable.Maximum() > 1)
                {
                    ImGuiComponents.ProgressBasedAchievement(unlockable);
                }
                else
                {
                    ImGuiComponents.SimpleAchievement(unlockable);
                }
            }

            if (achievement is AchievementLayoutItemTiered tiered)
            {
                var unlockable = Plugin.UnlockablesService.GetUnlockableTieredAchievement(tiered.Ids);
                if (searchBuffer != "" && !unlockable.NameLowercase.Contains(searchTerm))
                {
                    continue;
                }
                ImGuiComponents.MultiProgressBasedAchievement(unlockable);
            }

            if (achievement is AchievementLayoutItemCombined combined)
            {
                // currently not handled, TODO
                foreach (var simpleAchievement in combined.Ids)
                {
                    var unlockable = Plugin.UnlockablesService.GetUnlockableAchievement(simpleAchievement);
                    if (searchBuffer != "" && !unlockable.NameLowercase.Contains(searchTerm))
                    {
                        continue;
                    }
                    if (unlockable.Maximum() != null && unlockable.Maximum() > 1)
                    {
                        ImGuiComponents.ProgressBasedAchievement(unlockable);
                    }
                    else
                    {
                        ImGuiComponents.SimpleAchievement(unlockable);
                    }
                }
            }

            if (achievement != selectedLayoutCategory.Items.Last())
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
                                                (selectedLayoutCategory == category ? ImGuiTreeNodeFlags.Selected : 0)))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    selectedLayoutCategory = category;
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

            foreach (var layout in plugin.MainWindowLayout.AchievementLayout)
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
