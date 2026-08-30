using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Component;

public static class UiSize {
    public const float StatusBarHeight = 32f;

    public static float Em(float em) {
        return Plugin.PluginInterface.UiBuilder.FontDefaultSizePx * em;
    }

    public static float MainContentHeight(Configuration configuration) {
        return ImGui.GetContentRegionAvail().Y - (configuration.DebugMode ? StatusBarHeight : 0f);
    }
}
