using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using MethodTimer;
using Recollection.Data;
using Recollection.Data.Unlockable;
using Recollection.External.Lalachievements;
using Recollection.Helpers;
using Sheets = Lumina.Excel.Sheets;

namespace Recollection.Services;

public class UnlockablesService : IDisposable {
    private readonly IPluginLog log = Plugin.GetLogger<UnlockablesService>();
    private readonly Plugin plugin;
    private readonly LalachievementsService lalachievementsService;
    private readonly ConcurrentDictionary<uint, UnlockableAchievement> achievements = new();
    private readonly ConcurrentDictionary<uint, UnlockableTieredAchievement> tieredAchievements = new();
    private readonly ConcurrentDictionary<UnlockableKey, IUnlockable> collectionItems = new();
    public readonly Dictionary<uint, uint> HighestAchievementIdMap;
    private readonly Dictionary<uint, (uint eNpcBaseRowId, uint levelRowId)> ttLinkCache = BuildTripleTriadLinkCache();
    private readonly Dictionary<uint, uint> gatheringNodeCache = BuildGatheringNodeCache();
    private readonly ConcurrentQueue<UnlockableKey> unlockedChanges = new();
    private ulong beatenTripleTriadNpcsHash;
    private ulong gatheredGatheringItemsHash;
    private ulong completedLevesHash;
    private ulong huntingLogRankDataHash;
    private ulong completedSurveyRecordsHash;

    private bool achievementsWereLoaded = false;

    public UnlockablesService(Plugin plugin) {
        this.plugin = plugin;
        lalachievementsService = plugin.LalachievementsService;
        HighestAchievementIdMap = plugin.MainLayout.GetHighestIdMap();
        Plugin.UnlockState.Unlock += OnUnlock;
        Plugin.ClientState.Logout += OnLogout;
    }

    private void OnLogout(int type, int code) {
        achievementsWereLoaded = false;
    }

    public void Dispose() {
        Plugin.UnlockState.Unlock -= OnUnlock;
        Plugin.ClientState.Logout -= OnLogout;
    }

    private void OnUnlock(RowRef rowRef) {
        var id = rowRef.RowId;

        if (rowRef.TryGetValue(out Sheets.Achievement achievement)) {
            OnAchievementUnlock(achievement);
            return;
        }

        IUnlockable? unlockable = rowRef switch {
            _ when rowRef.Is<Sheets.Mount>() => collectionItems.GetValueOrDefault(new(UnlockableType.Mount, id)),
            _ when rowRef.Is<Sheets.Companion>() => collectionItems.GetValueOrDefault(new(UnlockableType.Minion, id)),
            _ when rowRef.Is<Sheets.Title>() => collectionItems.GetValueOrDefault(new(UnlockableType.Title, id)),
            _ when rowRef.Is<Sheets.TripleTriadCard>() => collectionItems.GetValueOrDefault(new(UnlockableType.TripleTriadCard, id)),
            _ when rowRef.Is<Sheets.BuddyEquip>() => collectionItems.GetValueOrDefault(new(UnlockableType.Barding, id)),
            _ when rowRef.Is<Sheets.Ornament>() => collectionItems.GetValueOrDefault(new(UnlockableType.FashionAccessory, id)),
            _ when rowRef.Is<Sheets.GlassesStyle>() => collectionItems.GetValueOrDefault(new(UnlockableType.Facewear, id)),
            _ when rowRef.Is<Sheets.Emote>() => collectionItems.GetValueOrDefault(new(UnlockableType.Emote, id)),
            _ when rowRef.Is<Sheets.Orchestrion>() => collectionItems.GetValueOrDefault(new(UnlockableType.OrchestrionRoll, id)),
            _ when rowRef.Is<Sheets.Recipe>() => collectionItems.GetValueOrDefault(new(UnlockableType.CraftingLog, id)),
            _ when rowRef.Is<Sheets.MKDLore>() => collectionItems.GetValueOrDefault(new(UnlockableType.OccultRecord, id)),
            _ when rowRef.Is<Sheets.Item>() => collectionItems.GetValueOrDefault(new(UnlockableType.FramersKit, id)),
            _ when rowRef.GetValueOrDefault<Sheets.CharaMakeCustomize>() is { } customize =>
                collectionItems.GetValueOrDefault(new(UnlockableType.Hairstyle, customize.FeatureID)),
            _ => null
        };

        if (unlockable != null) {
            unlockable.Unlocked = true;
            unlockedChanges.Enqueue(new UnlockableKey(unlockable.Type, unlockable.Id));
        }
    }

    private void OnAchievementUnlock(Sheets.Achievement achievement) {
        achievements.GetValueOrDefault(achievement.RowId)?.Unlocked = true;

        if (tieredAchievements.TryGetValue(achievement.RowId, out var tiered)) {
            tiered.UpdateCurrent();
            tiered.Unlocked = tiered.Current == tiered.Maximum;
        }

        unlockedChanges.Enqueue(new UnlockableKey(UnlockableType.Achievement, achievement.RowId));
    }

    public void SetAchievementProgress(uint achievementId, uint progress) {
        if (achievements.TryGetValue(achievementId, out var achievement)) {
            achievement.Current = progress;
        }
    }

    public unsafe void CheckPolledUnlocks() {
        if (Plugin.UiState.Valid && HashChanged(ref beatenTripleTriadNpcsHash, Plugin.UiState.Value.BeatenTripleTriadResidentsBitArray.ComputeHash())) {
            UpdatePolledUnlockables(UnlockableType.TripleTriadNpc, (_, unlockable) => {
                unlockable.Unlocked = Plugin.UiState.Value.IsTripleTriadNpcBeaten(unlockable.Id);
            });
        }

        if (Plugin.QuestManager.Valid && HashChanged(ref gatheredGatheringItemsHash, Plugin.QuestManager.Value.GatheredGatheringItemsBitArray.ComputeHash())) {
            UpdatePolledUnlockables(UnlockableType.GatheringLog, (key, unlockable) => {
                unlockable.Unlocked = FFXIVClientStructs.FFXIV.Client.Game.QuestManager.IsGatheringItemGathered((ushort)key.Id);
            });
        }

        if (Plugin.QuestManager.Valid && HashChanged(ref completedLevesHash, Plugin.QuestManager.Value.CompletedLeveQuestsBitArray.ComputeHash())) {
            UpdatePolledUnlockables(UnlockableType.Leve, (key, unlockable) => {
                unlockable.Unlocked = Plugin.UnlockState.IsLeveCompleted(ExcelSheets.Leve.Value.GetRow(key.Id));
            });
        }

        if (Plugin.NativePlayerState.Valid && HashChanged(ref completedSurveyRecordsHash, Plugin.NativePlayerState.Value.CompletedVVDNotebookContentsBitArray.ComputeHash())) {
            UpdatePolledUnlockables(UnlockableType.SurveyRecord, (key, unlockable) => {
                unlockable.Unlocked = UnlockableSurveyRecord.IsCompleted(key.Id);
            });
        }

        if (Plugin.MonsterNoteManager.Valid) {
            var rankData = Plugin.MonsterNoteManager.Value.RankData;
            fixed (void* ptr = rankData) {
                var bitArray = new InteropGenerator.Runtime.BitArray((byte*)ptr, rankData.Length * sizeof(FFXIVClientStructs.FFXIV.Client.Game.MonsterNoteRankInfo) * 8);
                if (HashChanged(ref huntingLogRankDataHash, bitArray.ComputeHash())) {
                    UpdatePolledUnlockables(UnlockableType.HuntingLog, (key, unlockable) => {
                        var note = ExcelSheets.MonsterNote.Value.GetRow(key.Id / 10);
                        var target = note.MonsterNoteTarget[(int)key.Id % 10].Value;
                        var type = HuntingLogType.GetByMonsterNoteRowId(note.RowId);
                        unlockable.Current = (uint)UnlockableHuntingLog.GetCurrent(type, note, target, note.Count[(int)key.Id % 10]);
                        unlockable.Unlocked = unlockable.Current == unlockable.Maximum;
                    });
                }
            }
        }
    }

    private static bool HashChanged(ref ulong previousHash, ulong hash) {
        if (previousHash == hash) return false;
        previousHash = hash;
        return true;
    }

    private void UpdatePolledUnlockables(UnlockableType type, Action<UnlockableKey, IUnlockable> update) {
        foreach (var (key, unlockable) in collectionItems) {
            if (key.Type != type) continue;

            var wasUnlocked = unlockable.Unlocked;
            update(key, unlockable);
            if (unlockable.Unlocked != wasUnlocked) unlockedChanges.Enqueue(key);
        }
    }

    public bool HasChanges => !unlockedChanges.IsEmpty;

    public List<UnlockableKey> GetUpdates() {
        var changes = new List<UnlockableKey>();
        while (unlockedChanges.TryDequeue(out var key)) changes.Add(key);
        return changes;
    }

    [Time]
    private static Dictionary<uint, (uint eNpcBaseRowId, uint levelRowId)> BuildTripleTriadLinkCache() {
        var eNpcBaseToLevel = new Dictionary<uint, uint>();
        var map = new Dictionary<uint, (uint eNpcBaseRowId, uint levelRowId)>();
        var tripleTriadSheet = ExcelSheets.TripleTriad.Value;

        foreach (var level in ExcelSheets.Level.Value) {
            eNpcBaseToLevel.TryAdd(level.Object.RowId, level.RowId);
        }

        foreach (var eNpcBase in ExcelSheets.ENpcBase.Value) {
            foreach (var data in eNpcBase.ENpcData) {
                if (tripleTriadSheet.HasRow(data.RowId)) {
                    var levelRowId = eNpcBaseToLevel.TryGetValue(eNpcBase.RowId, out var lvl) ? lvl : 0;
                    map[data.RowId] = (eNpcBase.RowId, levelRowId);
                }
            }
        }

        return map;
    }

    [Time]
    private static Dictionary<uint, uint> BuildGatheringNodeCache() =>
        ExcelSheets.GatheringPoint.Value
                   .Where(it => it.IsValidEntry())
                   .SelectMany(it => it.GatheringPointBase.Value.Item
                                       .Where(item => item.RowId > 0)
                                       .Select(item => (item.RowId, it.RowId)))
                   .GroupBy(it => it.Item1)
                   .Select(it => it.First(item => item.Item2 > 0))
                   .ToDictionary();

    private ITableRow GetTableRow<T>(uint id) where T : ITableRow {
        return lalachievementsService.GetTable<T>().First(it => it.Id == id);
    }

    [Time]
    public UnlockableAchievement GetUnlockableAchievement(uint achievementId) {
        if (achievements.TryGetValue(achievementId, out var it)) {
            return it;
        }

        var unlockable = new UnlockableAchievement(ExcelSheets.Achievement.Value.GetRow(achievementId), plugin);
        achievements[achievementId] = unlockable;
        return unlockable;
    }

    [Time]
    public UnlockableTieredAchievement GetUnlockableTieredAchievement(List<uint> achievementIds, bool spoilers) {
        if (tieredAchievements.TryGetValue(achievementIds.Last(), out var it)) {
            return it;
        }

        var achievementList = achievementIds.Select(id => ExcelSheets.Achievement.Value.GetRow(id)).ToList();
        var providesAchievements = achievementIds.Select(GetUnlockableAchievement).ToList();
        var unlockable = new UnlockableTieredAchievement(achievementList, providesAchievements, spoilers, plugin);
        achievementIds.ForEach(id => tieredAchievements[id] = unlockable);
        return unlockable;
    }

    [Time]
    private UnlockableTripleTriadNpc CreateUnlockableTripleTriadNpc(UnlockableKey key) {
        var offset = ExcelSheets.TripleTriad.Value.First().RowId;
        var tt = ExcelSheets.TripleTriad.Value.GetRow(offset + key.Id);
        var ttResident = ExcelSheets.TripleTriadResident.Value.GetRow(tt.RowId);
        if (!ttLinkCache.TryGetValue(tt.RowId, out var match)) {
            throw new InvalidDataException($"Could not match any ENpcBase entry to the TripleTriadResident {ttResident.RowId}");
        }

        var eNpcBase = ExcelSheets.ENpcBase.Value.GetRow(match.eNpcBaseRowId);
        var eNpcResident = ExcelSheets.ENpcResident.Value.GetRow(eNpcBase.RowId);
        if (match.levelRowId == 0) {
            throw new InvalidDataException($"Could not match any Level entry to the ENpcBase {eNpcBase.RowId}");
        }

        var level = ExcelSheets.Level.Value.GetRow(match.levelRowId);
        return new UnlockableTripleTriadNpc(tt, ttResident, eNpcResident, level);
    }

    [Time]
    private UnlockableGatheringLog CreateUnlockableGatheringLog(UnlockableKey key) {
        var point = ExcelSheets.GatheringPoint.Value.GetRow(gatheringNodeCache[key.Id]);
        var item = ExcelSheets.GatheringItem.Value.GetRow(key.Id);
        var exported = ExcelSheets.ExportedGatheringPoint.Value.GetRow(point.GatheringPointBase.RowId);

        return new(item, point, exported);
    }

    [Time]
    private UnlockableHuntingLog CreateUnlockableHuntingLog(UnlockableKey key) {
        // see how hunting log categories are built for why divided by 10 and modulo 10
        var note = ExcelSheets.MonsterNote.Value.GetRow(key.Id / 10);
        var target = note.MonsterNoteTarget[(int)key.Id % 10].Value;
        var type = HuntingLogType.GetByMonsterNoteRowId(note.RowId);

        return new(type, note, target, note.Count[(int)key.Id % 10]);
    }

    [Time]
    private IUnlockable CreateUnlockable(UnlockableKey key) => key.Type switch {
        UnlockableType.Mount =>
            new UnlockableMount(ExcelSheets.Mount.Value.GetRow(key.Id), GetTableRow<Mount>(key.Id)),
        UnlockableType.Minion =>
            new UnlockableMinion(ExcelSheets.Companion.Value.GetRow(key.Id), GetTableRow<Minion>(key.Id)),
        UnlockableType.Title =>
            new UnlockableTitle(ExcelSheets.Title.Value.GetRow(key.Id)),
        UnlockableType.TripleTriadCard =>
            new UnlockableTripleTriadCard(ExcelSheets.TripleTriadCard.Value.GetRow(key.Id), GetTableRow<TripleTriadCard>(key.Id)),
        UnlockableType.TripleTriadNpc =>
            CreateUnlockableTripleTriadNpc(key),
        UnlockableType.Barding =>
            new UnlockableBarding(ExcelSheets.BuddyEquip.Value.GetRow(key.Id), GetTableRow<Barding>(key.Id)),
        UnlockableType.FashionAccessory =>
            new UnlockableFashionAccessory(ExcelSheets.Ornament.Value.GetRow(key.Id), GetTableRow<Fashion>(key.Id)),
        UnlockableType.Hairstyle =>
            new UnlockableHairstyle(ExcelSheets.CharaMakeCustomize.Value.First(it => it.FeatureID == key.Id), GetTableRow<Hair>(key.Id)),
        UnlockableType.Facewear =>
            new UnlockableFacewear(ExcelSheets.GlassesStyle.Value.GetRow(key.Id), GetTableRow<Spectacle>(key.Id)),
        UnlockableType.Emote =>
            new UnlockableEmote(ExcelSheets.Emote.Value.GetRow(key.Id), GetTableRow<Emote>(key.Id)),
        UnlockableType.Leve =>
            new UnlockableLeve(ExcelSheets.Leve.Value.GetRow(key.Id)),
        UnlockableType.FramersKit =>
            new UnlockableFramersKit(ExcelSheets.Item.Value.GetRow(key.Id)),
        UnlockableType.OccultRecord =>
            new UnlockableOccultRecord(ExcelSheets.MKDLore.Value.GetRow(key.Id)),
        UnlockableType.SurveyRecord =>
            new UnlockableSurveyRecord(ExcelSheets.VVDNotebookContents.Value.GetRow(key.Id)),
        UnlockableType.CraftingLog =>
            new UnlockableCraftingLog(ExcelSheets.Recipe.Value.GetRow(key.Id)),
        UnlockableType.GatheringLog =>
            CreateUnlockableGatheringLog(key),
        UnlockableType.OrchestrionRoll =>
            new UnlockableOrchestrionRoll(ExcelSheets.Orchestrion.Value.GetRow(key.Id)),
        UnlockableType.HuntingLog =>
            CreateUnlockableHuntingLog(key),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unimplemented unlockable type")
    };

    public IUnlockable GetUnlockable(UnlockableType type, uint rowId) => collectionItems.GetOrAdd(new UnlockableKey(type, rowId), CreateUnlockable);

    public IUnlockable? GetExistingAchievement(uint achievementId) {
        return achievements.GetValueOrDefault(achievementId) as IUnlockable ?? tieredAchievements.GetValueOrDefault(achievementId);
    }

    public List<IUnlockable> GetPinnedUnlockables() {
        return plugin.Configuration.PinnedAchievements
                     .Select(id => GetExistingAchievement(id) ?? GetUnlockableAchievement(id))
                     .ToList();
    }

    [Time]
    public AchievementProgress CalculateAchievementProgress(IEnumerable<uint> achievementIds) {
        uint obtainedCount = 0;
        uint totalCount = 0;
        uint obtainedPoints = 0;
        uint totalPoints = 0;

        foreach (var id in achievementIds) {
            var achievement = GetUnlockableAchievement(id);
            totalCount++;
            totalPoints += achievement.Points;
            if (!achievement.Unlocked) continue;

            obtainedCount++;
            obtainedPoints += achievement.Points;
        }

        return new AchievementProgress(new Score(obtainedCount, totalCount), new Score(obtainedPoints, totalPoints));
    }

    [Time]
    public static Score CalculateAchievementPoints() {
        uint obtained = 0;
        uint total = 0;

        foreach (var achievement in ExcelSheets.Achievement.Value) {
            total += achievement.Points;
            if (Plugin.UnlockState.IsAchievementComplete(achievement)) {
                obtained += achievement.Points;
            }
        }

        return new Score(obtained, total);
    }

    public bool GetAchievementListUpdatedForUi() {
        if (!achievementsWereLoaded && Plugin.UnlockState.IsAchievementListLoaded) {
            achievementsWereLoaded = true;
            return true;
        }

        return false;
    }

    public void Refresh() {
        log.Information("Refreshed unlockables");
        achievements.Clear();
        tieredAchievements.Clear();
        collectionItems.Clear();
        unlockedChanges.Clear();
    }
}
