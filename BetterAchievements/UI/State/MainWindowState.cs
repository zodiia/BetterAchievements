using System;

namespace BetterAchievements.UI.State;

public enum ConfigurationEffect {
    None,
    RebuildView,
    Refilter
}

public class MainWindowState {
    private readonly Configuration configuration;

    public readonly UnlockablesState Unlockables;
    public readonly NavigationState Navigation;
    public readonly DebugState FrameTimes;

    public string SearchBuffer = "";

    public MainWindowState(Plugin plugin) {
        configuration = plugin.Configuration;
        Unlockables = new UnlockablesState(plugin);
        Navigation = new NavigationState(plugin, Unlockables);
        FrameTimes = new DebugState(configuration);
    }

    public void SetSearch(string search) {
        Unlockables.SetSearch(search);
        Navigation.Rebuild();
    }

    public void Refresh() {
        Unlockables.Refresh();
        Navigation.Rebuild();
    }

    public void CheckForUiRefresh() {
        if (Unlockables.CheckForUpdates()) Navigation.Rebuild();
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
