namespace BetterAchievements.UI.Component;

public static class UiSize
{
    public static float Em(float em)
    {
        return Plugin.PluginInterface.UiBuilder.FontDefaultSizePx * em;
    }

}
