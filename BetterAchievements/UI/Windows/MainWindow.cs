using System;
using System.Linq;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.Windows.Views;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace BetterAchievements.UI.Windows;

public class MainWindow : Window, IDisposable {
    private const float MinSidebarWidth = 350;

    private readonly Plugin plugin;
    private readonly MainWindowState state;
    private float sidebarWidth = MinSidebarWidth;

    private AchievementsView achievementsView;

    public MainWindow(Plugin plugin)
        : base($"Better Achievements v{plugin.PluginManifest.AssemblyVersion}") {
        this.plugin = plugin;
        state = new MainWindowState(plugin);
        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(900, 450),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        achievementsView = new AchievementsView(state, plugin);
    }

    public void Dispose() { }

    private void DrawStatusBar() {
        if (!state.Configuration.DebugMode) return;

        using var statusBar = ImRaii.Child("StatusBar", ImGui.GetContentRegionAvail() with { Y = 32 }, true);
        if (!statusBar) return;

        ImGui.Text($"frame time {state.DebugStopwatch.Elapsed.Microseconds / 1000.0}ms/f");
    }

    public override void Draw() {
        state.DebugStart();
        state.CheckForUiRefresh();
        UiComponents.Topbar(state);

        var maxSidebarWidth = MinSidebarWidth + Math.Max(0, ImGui.GetWindowWidth() - (SizeConstraints!.Value.MinimumSize.X * 1.5f)); // todo: find out where the scale is stored
        sidebarWidth = Math.Clamp(sidebarWidth, MinSidebarWidth, maxSidebarWidth);
        var sidebarHeight = ImGui.GetContentRegionAvail().Y - (state.Configuration.DebugMode ? 32 : 0);

        UiComponents.Sidebar(state, sidebarWidth);
        UiComponents.VerticalSplitter("##SidebarSplitter", ref sidebarWidth, MinSidebarWidth, maxSidebarWidth, sidebarHeight, 16F);
        achievementsView.Draw();
        DrawStatusBar();
        state.DebugEnd();
    }
}
