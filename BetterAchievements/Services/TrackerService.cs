using System;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.External.Mapping;
using Dalamud.Game.DutyState;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using BattleNpcSubKind = FFXIVClientStructs.FFXIV.Client.Game.Object.BattleNpcSubKind;
using ObjectKind = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind;

namespace BetterAchievements.Services;

public class TrackerService {
    public static readonly IPluginLog Log = Plugin.GetLogger<TrackerService>();

    private readonly Plugin plugin;

    public TrackerService(Plugin plugin) {
        this.plugin = plugin;
        SetupEvents();
    }

    private uint GetLastAchievementInSeries(AchievementIdMap achievement) =>
        plugin.UnlockablesService.GetExistingAchievement((uint)achievement)?.Id() ?? 0;

    public unsafe void SetupEvents() {
        Plugin.DutyState.DutyCompleted += OnDutyCompleted;
        plugin.SetModeHook.OnDetour += OnCharacterSetMode;
        plugin.AddonLifecycleService.OnFateCompleted += OnFateCompleted;
    }

    private void OnDutyCompleted(IDutyStateEventArgs args) {
        Log.Information("Territory type: {T}", args.TerritoryType.Value.Name.ToString());
        Log.Information("Content finder condition: {C}", args.ContentFinderCondition.Value.Name.ToString());
    }

    private unsafe void OnCharacterSetMode(Character* chara, CharacterModes mode, byte modeParam) {
        if (chara == null) return;
        switch (chara->ObjectKind, chara->BattleNpcSubKind, mode, chara->CombatTagType) {
            case (ObjectKind.BattleNpc, BattleNpcSubKind.Combatant, CharacterModes.Dead, 1 /* Character Tag */):
                try {
                    var combatTagger = Plugin.ObjectTable.CharacterManagerObjects.FirstOrDefault(it => it.GameObjectId == chara->CombatTaggerId.Id);
                    if (combatTagger == null) break;
                    if (Plugin.ObjectTable.LocalPlayer?.EntityId == combatTagger.EntityId
                        || Plugin.PartyList.Any(it => it.EntityId == combatTagger.EntityId)) {
                        plugin.AchievementProgressService.IncrementProgress(GetLastAchievementInSeries(AchievementIdMap.ToCrushYourEnemiesI), 1);
                    }
                } catch (Exception ex) {
                    Log.Error(ex, "Error while trying to parse Character.SetMode");
                }

                break;
            case (ObjectKind.BattleNpc, BattleNpcSubKind.Combatant, CharacterModes.Dead, 2 /* Party Tag */):
                if ((ulong)Plugin.PartyList.PartyId == chara->CombatTaggerId.Id) {
                    plugin.AchievementProgressService.IncrementProgress(GetLastAchievementInSeries(AchievementIdMap.ToCrushYourEnemiesI), 1);
                }

                break;
        }
    }

    private void OnFateCompleted(Fate fate, FateMedal medal) {
        if (medal == FateMedal.Gold) {
            plugin.AchievementProgressService.IncrementProgress((uint)AchievementIdMap.DateWithDestinyI, 1);
        }
    }
}
