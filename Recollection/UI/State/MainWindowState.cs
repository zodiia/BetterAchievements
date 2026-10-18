using System;
using Recollection.Data;
using Recollection.UI.Windows;

namespace Recollection.UI.State;

public class MainWindowState {
    private readonly Configuration configuration;

    public readonly UnlockablesState Unlockables;
    public readonly RouletteState Roulette;
    public readonly NavigationState Navigation;

    public string SearchBuffer = "";

    public MainWindowState(Plugin plugin) {
        configuration = plugin.Configuration;
        Unlockables = new UnlockablesState(plugin);
        Roulette = new RouletteState(plugin);
        Navigation = new NavigationState(plugin, Unlockables, Roulette);
        Plugin.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    public void Dispose() {
        Plugin.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Roulette.Dispose();
    }

    private void OnTerritoryChanged(uint territory) {
        if (configuration.AreaFilter == AreaFilter.All) return;

        Unlockables.ApplyFilters();
        Navigation.Rebuild();
    }

    public void SetSearch(string search) {
        Unlockables.SetSearch(search);
        Navigation.Rebuild();
    }

    public void Refresh() {
        Unlockables.Refresh();
        Roulette.Rebuild();
        Navigation.Rebuild();
    }

    public void CheckForUiRefresh() {
        if (!Unlockables.CheckForUpdates()) return;

        Roulette.Rebuild();
        Navigation.Rebuild();
    }

    public void UpdateConfiguration(Action<Configuration> update, ConfigurationEffect effect = ConfigurationEffect.None) {
        update(configuration);
        configuration.Save();

        switch (effect) {
            case ConfigurationEffect.Refilter:
                Unlockables.ApplyFilters();
                Navigation.Rebuild();
                break;

            case ConfigurationEffect.RebuildView:
                Navigation.Rebuild();
                break;
        }
    }
}
