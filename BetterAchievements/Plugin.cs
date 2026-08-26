using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using System.Text.Json;
using BetterAchievements.Data;
using BetterAchievements.Hooks;
using BetterAchievements.External.Lalachievements;
using BetterAchievements.Services;
using BetterAchievements.UI.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Achievement = Lumina.Excel.Sheets.Achievement;

namespace BetterAchievements;

public sealed class Plugin : IDalamudPlugin {
    // private static readonly ILogger Log = GetLogger<Plugin>();

    private const string CommandName = "/betterachievements";
    private const string CommandAlias = "/bach";
    private const string CommandHelp = "Open the main achievements interface";

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    [PluginService]
    internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    [PluginService]
    internal static IUnlockState UnlockState { get; private set; } = null!;

    [PluginService]
    internal static IDutyState DutyState { get; private set; } = null!;

    [PluginService]
    internal static IObjectTable ObjectTable { get; private set; } = null!;

    [PluginService]
    internal static IPartyList PartyList { get; private set; } = null!;

    [PluginService]
    internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    [PluginService]
    internal static IFateTable FateTable { get; private set; } = null!;

    [PluginService]
    internal static IPlayerState PlayerState { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    internal static unsafe UIState* UiState { get; } = UIState.Instance();

    public UnlockablesProgressService UnlockablesProgressService { get; private set; }
    public UnlockablesService UnlockablesService { get; private set; }
    public LalachievementsService LalachievementsService { get; private set; }
    public ProgressTrackerService ProgressTrackerService { get; private set; }
    public AddonLifecycleService AddonLifecycleService { get; private set; }

    public Configuration Configuration { get; private set; }

    public readonly WindowSystem WindowSystem = new("BetterAchievements");
    private MainWindow MainWindow { get; init; }
    public MainLayout MainLayout { get; init; }

    public ReceiveAchievementProgressHook ReceiveAchievementProgressHook { get; private set; } = null!;
    public SetModeHook SetModeHook { get; private set; } = null!;

    public Plugin() {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        foreach (var plugin in PluginInterface.InstalledPlugins)
        {
            if (plugin.Name == "BetterAchievements" || plugin.InternalName == "BetterAchievements") {
                Log.Info($"{plugin.Name}, {plugin.InternalName}, {plugin.Version}, {plugin.Manifest.AssemblyVersion}, {plugin.Manifest.Description}");
            }
        }

        try {
            ReceiveAchievementProgressHook = new ReceiveAchievementProgressHook();
            SetModeHook = new SetModeHook();
        } catch (Exception e) {
            Log.Error(e, "Hooks failed. If you see this, please contact the plugin author.");
        }

        AddonLifecycleService = new AddonLifecycleService();
        UnlockablesProgressService = new UnlockablesProgressService(this);
        UnlockablesService = new UnlockablesService(this);
        LalachievementsService = new LalachievementsService();
        ProgressTrackerService = new ProgressTrackerService(this);

        MainLayout = LoadMainWindowLayout();

        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) { HelpMessage = CommandHelp });
        CommandManager.AddHandler(CommandAlias, new CommandInfo(OnCommand) { HelpMessage = CommandHelp });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        HandleWarnings();
    }

    public void Dispose() {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();

        ReceiveAchievementProgressHook.Dispose();
        SetModeHook.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args) {
        MainWindow.Toggle();
    }

    private void HandleWarnings() {
        MainLayout.CheckMissingAchievements(DataManager.Excel.GetSheet<Achievement>());
    }

    public void ToggleConfigUi() => MainWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();

    private static MainLayout LoadMainWindowLayout() {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, ReadCommentHandling = JsonCommentHandling.Skip };
        return JsonSerializer.Deserialize<MainLayout>(GetResourceFile("layout.jsonc"), options)!;
    }

    public static string GetResourceFile(string fileName) {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"BetterAchievements.Resources.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream ?? throw new InvalidOperationException());
        return reader.ReadToEnd();
    }

    public static IPluginLog GetLogger<T>() {
        return Log;
    }
}
