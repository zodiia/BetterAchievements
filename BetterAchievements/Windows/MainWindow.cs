using System;
using System.Numerics;
using BetterAchievements.Unlockables;
using BetterAchievements.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Achievement = Lumina.Excel.Sheets.Achievement;

namespace BetterAchievements.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private AchievementLayoutCategory? selectedLayoutCategory;

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

    public void DrawMainContent()
    {
        if (!ImGui.BeginChild("MainContent", Vector2.Zero, true))
        {
            return;
        }

        var achievements = Plugin.DataManager.GetExcelSheet<Achievement>();

        ImGuiComponents.SimpleAchievement(new UnlockableAchievement(achievements.GetRow(801)));
        ImGui.Separator();
        ImGuiComponents.SimpleAchievement(new UnlockableAchievement(achievements.GetRow(802)));
        ImGui.Separator();
        ImGuiComponents.ProgressBasedAchievement(new UnlockableAchievement(achievements.GetRow(23)));
        ImGui.Separator();
        ImGuiComponents.ProgressBasedAchievement(new UnlockableAchievement(achievements.GetRow(3429)));
        ImGui.Separator();
        ImGuiComponents.ProgressBasedAchievement(new UnlockableAchievement(achievements.GetRow(3430)));
        ImGui.Separator();
        ImGuiComponents.MultiProgressBasedAchievement(new UnlockableMultiAchievement(new()
        {
            achievements.GetRow(1), achievements.GetRow(2), achievements.GetRow(3), achievements.GetRow(4),
            achievements.GetRow(5), achievements.GetRow(6), achievements.GetRow(7), achievements.GetRow(1336)
        }, "Crush your Enemies"));
        ImGui.Separator();
        ImGuiComponents.MultiProgressBasedAchievement(new UnlockableMultiAchievement(new()
        {
            achievements.GetRow(1180), achievements.GetRow(1181), achievements.GetRow(1182), achievements.GetRow(1183),
            achievements.GetRow(1184), achievements.GetRow(2256), achievements.GetRow(2902), achievements.GetRow(3439)
        }, "Mean Machine"));
        ImGui.Separator();
        ImGuiComponents.MultiProgressBasedAchievement(new UnlockableMultiAchievement(new()
        {
            achievements.GetRow(1100), achievements.GetRow(1101), achievements.GetRow(1102), achievements.GetRow(1372),
            achievements.GetRow(1488), achievements.GetRow(1631), achievements.GetRow(1908), achievements.GetRow(2078),
            achievements.GetRow(2368), achievements.GetRow(2643), achievements.GetRow(3020), achievements.GetRow(3209),
            achievements.GetRow(3557), achievements.GetRow(3820)
        }, "Triple-decker"));

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
        if (ImGui.BeginChild("Sidebar",  Vector2.Zero with { X = 350 }, true))
        {
            if (!ImGui.CollapsingHeader("Achievements"))
            {
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
