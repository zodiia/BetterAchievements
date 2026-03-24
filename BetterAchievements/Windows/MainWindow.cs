using System;
using System.Collections.Generic;
using System.Numerics;
using BetterAchievements.Unlockables;
using BetterAchievements.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Achievement = Lumina.Excel.Sheets.Achievement;

namespace BetterAchievements.Windows;

public class MainWindow : Window, IDisposable
{
    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, IDataManager dataManager, IUnlockState unlockState)
        : base("Better Achievements")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 450),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (!ImGui.BeginChild("MainContent", Vector2.Zero))
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
}
