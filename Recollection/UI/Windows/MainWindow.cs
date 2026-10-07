using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Recollection.UI.Component;
using Recollection.UI.Component.Sidebar;
using Recollection.UI.State;

namespace Recollection.UI.Windows;

public class MainWindow : Window, IDisposable {
    private const float MinSidebarWidth = 350;

    private readonly Plugin plugin;
    private readonly MainWindowState state;
    private float sidebarWidth = MinSidebarWidth;

    public MainWindow(Plugin plugin)
        : base($"Recollection v{plugin.PluginManifest.AssemblyVersion}") {
        this.plugin = plugin;
        state = new MainWindowState(plugin);
        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(900, 450),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    public void OpenSettings() {
        IsOpen = true;
        state.Navigation.Navigate(new NavigationTarget.Settings());
    }

    public override void Draw() {
        state.CheckForUiRefresh();

        var maxSidebarWidth = MinSidebarWidth + Math.Max(0, ImGui.GetWindowWidth() - (SizeConstraints!.Value.MinimumSize.X * 1.5f)); // todo: find out where the scale is stored
        sidebarWidth = Math.Clamp(sidebarWidth, MinSidebarWidth, maxSidebarWidth);
        var sidebarHeight = ImGui.GetContentRegionAvail().Y;

        SidebarComponents.Sidebar(plugin, state, sidebarWidth);
        UiComponents.VerticalSplitter("##SidebarSplitter", ref sidebarWidth, MinSidebarWidth, maxSidebarWidth, sidebarHeight, 16F);
        state.Navigation.CurrentView.Draw();
    }
}
