using System;
using System.Numerics;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.Component.Sidebar;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace BetterAchievements.UI.Windows;

public class MainWindow : Window, IDisposable {
    private const float MinSidebarWidth = 350;

    private readonly Plugin plugin;
    private readonly MainWindowState state;
    private float sidebarWidth = MinSidebarWidth;

    public MainWindow(Plugin plugin)
        : base($"Better Achievements v{plugin.PluginManifest.AssemblyVersion}") {
        this.plugin = plugin;
        state = new MainWindowState(plugin);
        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(900, 450),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    private void DrawStatusBar() {
        if (!plugin.Configuration.DebugMode) return;

        using var statusBar = ImRaii.Child("StatusBar", ImGui.GetContentRegionAvail() with { Y = UiSize.StatusBarHeight }, true);
        if (!statusBar) return;

        ImGui.Text($"frame time {state.FrameTimes.AverageMs:F3}ms/f (highest {state.FrameTimes.WorstMs:F3}ms/f)");
    }

    public override void Draw() {
        state.FrameTimes.StartDebug();
        state.CheckForUiRefresh();

        var maxSidebarWidth = MinSidebarWidth + Math.Max(0, ImGui.GetWindowWidth() - (SizeConstraints!.Value.MinimumSize.X * 1.5f)); // todo: find out where the scale is stored
        sidebarWidth = Math.Clamp(sidebarWidth, MinSidebarWidth, maxSidebarWidth);
        var sidebarHeight = UiSize.MainContentHeight(plugin.Configuration);

        SidebarComponents.Sidebar(plugin, state, sidebarWidth);
        UiComponents.VerticalSplitter("##SidebarSplitter", ref sidebarWidth, MinSidebarWidth, maxSidebarWidth, sidebarHeight, 16F);
        state.Navigation.CurrentView.Draw();
        DrawStatusBar();
        state.FrameTimes.EndDebug();
    }
}
