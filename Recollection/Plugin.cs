using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Recollection.Data;
using Recollection.Hooks;
using Recollection.External.Lalachievements;
using Recollection.Helpers;
using Recollection.UI.Component;
using Recollection.UI.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Internal.Types.Manifest;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Recollection.Services;
using NativeMonsterNoteManager = FFXIVClientStructs.FFXIV.Client.Game.MonsterNoteManager;
using NativeQuestManager = FFXIVClientStructs.FFXIV.Client.Game.QuestManager;

namespace Recollection;

public sealed class Plugin : IDalamudPlugin {
    // private static readonly ILogger Log = GetLogger<Plugin>();

    private const string CommandName = "/recollection";
    private const string CommandAlias = "/rec";
    private const string CommandHelp = "Open the Recollection interface";
    private const string SettingsCommandName = "/recs";
    private const string SettingsCommandHelp = "Open Recollection directly to the settings";
    public const string Name = "Recollection";

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;

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

    internal static readonly NativeRef<NativeQuestManager> QuestManager = new(() => {
        unsafe {
            return NativeQuestManager.Instance();
        }
    });

    internal static readonly NativeRef<UIState> UiState = new(() => {
        unsafe {
            return UIState.Instance();
        }
    });

    internal static readonly NativeRef<NativeMonsterNoteManager> MonsterNoteManager = new(() => {
        unsafe {
            return NativeMonsterNoteManager.Instance();
        }
    });

    public IPluginManifest PluginManifest { get; private set; } = null!;

    public AchievementProgressService AchievementProgressService { get; private set; }
    public UnlockablesService UnlockablesService { get; private set; }
    public LalachievementsService LalachievementsService { get; private set; }
    public CollectionsService CollectionsService { get; private set; }
    public TrackerService TrackerService { get; private set; }
    public AddonLifecycleService AddonLifecycleService { get; private set; }
    public HistoryService HistoryService { get; private set; }

    public Configuration Configuration { get; private set; }

    public readonly WindowSystem WindowSystem = new(Name);
    private MainWindow MainWindow { get; init; }
    public MainLayout MainLayout { get; init; }

    public ReceiveAchievementProgressHook ReceiveAchievementProgressHook { get; private set; } = null!;
    public SetModeHook SetModeHook { get; private set; } = null!;

    public Plugin() {
        PluginInterface.ConfigDirectory.Create();
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        foreach (var plugin in PluginInterface.InstalledPlugins) {
            if (plugin.Name == Name || plugin.InternalName == Name) {
                PluginManifest = plugin.Manifest;
            }
        }

        try {
            ReceiveAchievementProgressHook = new ReceiveAchievementProgressHook();
            SetModeHook = new SetModeHook();
        } catch (Exception e) {
            Log.Error(e, "Hooks failed. If you see this, please contact the plugin author.");
        }

        MainLayout = LoadMainWindowLayout();

        LalachievementsService = new LalachievementsService();
        HistoryService = new HistoryService(this);
        AddonLifecycleService = new AddonLifecycleService();
        CollectionsService = new CollectionsService(this);
        UnlockablesService = new UnlockablesService(this);
        AchievementProgressService = new AchievementProgressService(this);
        TrackerService = new TrackerService(this);

        UiFonts.Initialize();

        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) { HelpMessage = CommandHelp });
        CommandManager.AddHandler(CommandAlias, new CommandInfo(OnCommand) { HelpMessage = CommandHelp });
        CommandManager.AddHandler(SettingsCommandName, new CommandInfo(OnSettingsCommand) { HelpMessage = SettingsCommandHelp });

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

        UiFonts.Dispose();

        CommandManager.RemoveHandler(CommandName);
        CommandManager.RemoveHandler(SettingsCommandName);
    }

    private void OnCommand(string command, string args) {
        MainWindow.Toggle();
    }

    private void OnSettingsCommand(string command, string args) {
        MainWindow.OpenSettings();
    }

    private void HandleWarnings() {
        MainLayout.CheckMissingAchievements(ExcelSheets.Achievement.Value);
    }

    public void ToggleConfigUi() => MainWindow.OpenSettings();
    public void ToggleMainUi() => MainWindow.Toggle();

    private static MainLayout LoadMainWindowLayout() {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, ReadCommentHandling = JsonCommentHandling.Skip };
        return JsonSerializer.Deserialize<MainLayout>(GetResourceFile("layout.jsonc"), options)!;
    }

    public static string GetResourceFile(string fileName) {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{Name}.Resources.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream ?? throw new InvalidOperationException());
        return reader.ReadToEnd();
    }

    // todo: better logging of which class is logging
    public static IPluginLog GetLogger<T>() {
        return Log;
    }
}
