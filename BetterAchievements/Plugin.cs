using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using BetterAchievements.Hooks;
using BetterAchievements.Unlockables;
using BetterAchievements.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace BetterAchievements;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    [PluginService]
    internal static IPlayerState PlayerState { get; private set; } = null!;

    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    [PluginService]
    internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    [PluginService]
    internal static IUnlockState UnlockState { get; private set; } = null!;

    internal static unsafe UIState* UiState { get; } = UIState.Instance();


    internal static readonly UnlockablesProgressService UnlockablesProgressService = new();

    private const string CommandName = "/betterachievements";
    private const string CommandAlias = "/bach";
    private const string CommandHelp = "Open the main achievements interface";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("BetterAchievements");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    private readonly AchievementProgressHook? achievementProgressHook;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        try
        {
            achievementProgressHook = new(GameInteropProvider);
        } catch (Exception e) {
            Log.Warning(e, "Could not hook achievement progress. This feature will be disabled.");
        }
        
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, DataManager, UnlockState);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) { HelpMessage = CommandHelp });
        CommandManager.AddHandler(CommandAlias, new CommandInfo(OnCommand) { HelpMessage = CommandHelp });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        Log.Information(GetResourceFile("layout.json"));
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        
        achievementProgressHook?.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    public string GetResourceFile(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"BetterAchievements.Data.{fileName}";
        
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream ?? throw new InvalidOperationException());
        return reader.ReadToEnd();
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
