using System;
using System.Linq;
using BetterAchievements.External.Mapping;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.DutyState;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Serilog;
using BattleNpcSubKind = FFXIVClientStructs.FFXIV.Client.Game.Object.BattleNpcSubKind;
using ObjectKind = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind;

namespace BetterAchievements.Services;

public class ProgressTrackerService
{
    private readonly Plugin plugin;

    public ProgressTrackerService(Plugin plugin)
    {
        this.plugin = plugin;
        SetupEvents();
    }

    private uint GetLastAchievementInSeries(AchievementIdMap achievement) =>
        plugin.UnlockablesService.GetExistingAchievement((uint)achievement)?.Id() ?? 0;

    public unsafe void SetupEvents()
    {
        Plugin.DutyState.DutyCompleted += OnDutyCompleted;
        plugin.SetModeHook.OnDetour += OnSetMode;
    }

    private void OnDutyCompleted(IDutyStateEventArgs args)
    {
        Log.Information("Territory type: {T}", args.TerritoryType.Value.Name.ToString());
        Log.Information("Content finder condition: {C}", args.ContentFinderCondition.Value.Name.ToString());
    }

    private unsafe void OnSetMode(Character* chara, CharacterModes mode, byte modeParam)
    {
        if (chara == null) return;
        switch (chara->ObjectKind, chara->BattleNpcSubKind, mode)
        {
            case (ObjectKind.BattleNpc, BattleNpcSubKind.Combatant, CharacterModes.Dead):
                try
                {
                    Log.Information("trying");
                    var combatTagger = Plugin.ObjectTable.CharacterManagerObjects.FirstOrDefault(it => it.GameObjectId == chara->CombatTaggerId.Id);
                    if (combatTagger == null) break;
                    Log.Information("found combat tagger: {N}", combatTagger.Name.ToString());
                    if (Plugin.ObjectTable.LocalPlayer?.EntityId == combatTagger.EntityId
                        || Plugin.PartyList.Any(it => it.EntityId == combatTagger.EntityId))
                    {
                        Log.Information("combat tagger IS either player or party!");
                        plugin.UnlockablesProgressService.IncrementProgress(GetLastAchievementInSeries(AchievementIdMap.ToCrushYourEnemiesI), 1);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error!");
                }
                break;
        }
    }

    private void OnEnemyKilled(IBattleNpc npc)
    {
        // if (!dead) return;
        // Log.Information("is dead");
        // // self counts as party member
        // if (!npc.StatusFlags.HasFlag(StatusFlags.PartyMember) || npc.StatusFlags.HasFlag(StatusFlags.AllianceMember)) return;
        // Log.Information("is party member");
        //
        // var zone = Plugin.DataManager.GetExcelSheet<TerritoryType>().GetRow(Plugin.ClientState.TerritoryType);
        // var bnpcName = Plugin.DataManager.GetExcelSheet<BNpcName>().GetRow(npc.BaseId);
        //
        // Log.Information("Kind: {Kind}, Zone: {Zone}, PlaceName: {PN}, PlaceNameRegion: {PNR}, PlaceNameZone: {PNZ}, BnpcName: {Bnpc}",
        //                 npc.BattleNpcKind, zone.Name.ToString(), zone.PlaceName.Value.Name.ToString(), zone.PlaceNameRegion.Value.Name.ToString(),
        //                 zone.PlaceNameZone.Value.Name.ToString(), bnpcName.Singular.ToString());
    }
}
