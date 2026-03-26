using System;
using System.Linq;
using System.Numerics;
using BetterAchievements.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private AchievementLayoutCategory? selectedLayoutCategory;

    private const string AchievementListNotLoadedWarning = "Achievement list not loaded.\nPlease open the vanilla achievement window once!";
    private const string NoCategorySelectedWarning = "Please select a category.";

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

    public void DrawMainContent()
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
                ImGuiComponents.MultiProgressBasedAchievement(Plugin.UnlockablesService.GetUnlockableTieredAchievement(tiered.Ids));
            }

            if (achievement is AchievementLayoutItemCombined combined)
            {
                // currently not handled, TODO
                foreach (var simpleAchievement in combined.Ids)
                {
                    var unlockable = Plugin.UnlockablesService.GetUnlockableAchievement(simpleAchievement);
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

    public void DrawSidebar()
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
        DrawSidebar();
        ImGui.SameLine();
        DrawMainContent();
    }
}
