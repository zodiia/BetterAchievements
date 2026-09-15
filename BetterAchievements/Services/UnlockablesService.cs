using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.External.Lalachievements;
using BetterAchievements.Helpers;
using Lumina.Excel;
using Serilog;

namespace BetterAchievements.Services;

public class UnlockablesService {
    private readonly Plugin plugin;
    private readonly LalachievementsService lalachievementsService;
    private readonly ConcurrentDictionary<uint, UnlockableAchievement> achievements = new();
    private readonly ConcurrentDictionary<uint, UnlockableTieredAchievement> tieredAchievements = new();
    private readonly ConcurrentDictionary<UnlockableKey, IUnlockable> collectionItems = new();
    public readonly Dictionary<uint, uint> HighestAchievementIdMap;
    private readonly Lazy<Dictionary<uint, (uint eNpcBaseRowId, uint levelRowId)>> ttLinkCache = new(BuildTripleTriadLinkCache);

    private readonly Lazy<Dictionary<uint, uint>> gatheringNodeCache = new(() => ExcelSheets.GatheringPoint.Value
                                                                                            .Where(it => it.IsValidEntry())
                                                                                            .SelectMany(it => it.GatheringPointBase.Value.Item
                                                                                                            .Where(item => item.RowId > 0)
                                                                                                            .Select(item => (item.RowId, it.RowId)))
                                                                                            .GroupBy(it => it.Item1)
                                                                                            .Select(it => it.First(item => item.Item2 > 0))
                                                                                            .ToDictionary());

    private bool unlocksUpdatedForUi = false;
    private bool achievementsWereLoaded = false;

    public UnlockablesService(Plugin plugin) {
        this.plugin = plugin;
        lalachievementsService = plugin.LalachievementsService;
        HighestAchievementIdMap = plugin.MainLayout.GetHighestIdMap();
        Plugin.UnlockState.Unlock += OnUnlock;
    }

    private void OnUnlock(RowRef _) {
        unlocksUpdatedForUi = true;
    }

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

    private ITableRow GetTableRow<T>(uint id) where T : ITableRow {
        return lalachievementsService.GetTable<T>().First(it => it.Id == id);
    }

    public UnlockableAchievement GetUnlockableAchievement(uint achievementId) {
        if (achievements.TryGetValue(achievementId, out var it)) {
            return it;
        }

        var unlockable = new UnlockableAchievement(ExcelSheets.Achievement.Value.GetRow(achievementId), plugin);
        achievements[achievementId] = unlockable;
        return unlockable;
    }

    public UnlockableTieredAchievement GetUnlockableTieredAchievement(List<uint> achievementIds, bool spoilers) {
        if (tieredAchievements.TryGetValue(achievementIds.Last(), out var it)) {
            return it;
        }

        var achievementList = achievementIds.Select(id => ExcelSheets.Achievement.Value.GetRow(id)).ToList();
        var unlockable = new UnlockableTieredAchievement(achievementList, spoilers, plugin);
        achievementIds.ForEach(id => tieredAchievements[id] = unlockable);
        return unlockable;
    }

    private UnlockableTripleTriadNpc CreateUnlockableTripleTriadNpc(UnlockableKey key) {
        var offset = ExcelSheets.TripleTriad.Value.First().RowId;
        var tt = ExcelSheets.TripleTriad.Value.GetRow(offset + key.Id);
        var ttResident = ExcelSheets.TripleTriadResident.Value.GetRow(tt.RowId);
        if (!ttLinkCache.Value.TryGetValue(tt.RowId, out var match)) {
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

    private UnlockableGatheringLog CreateUnlockableGatheringLog(UnlockableKey key) {
        var point = ExcelSheets.GatheringPoint.Value.GetRow(gatheringNodeCache.Value[key.Id]);
        var item = ExcelSheets.GatheringItem.Value.GetRow(key.Id);
        var exported = ExcelSheets.ExportedGatheringPoint.Value.GetRow(point.GatheringPointBase.RowId);

        return new(item, point, exported);
    }

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
        UnlockableType.CraftingLog =>
            new UnlockableCraftingLog(ExcelSheets.Recipe.Value.GetRow(key.Id)),
        UnlockableType.GatheringLog =>
            CreateUnlockableGatheringLog(key),
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

    public PointsScore CalculateAchievementPoints(IEnumerable<uint> achievementIds) {
        uint obtained = 0;
        uint total = 0;

        foreach (var id in achievementIds) {
            var achievement = GetUnlockableAchievement(id);
            total += achievement.Points();
            if (achievement.Unlocked()) obtained += achievement.Points();
        }

        return new PointsScore(obtained, total);
    }

    public PointsScore CalculateAchievementCount(IEnumerable<uint> achievementIds) {
        uint obtained = 0;
        uint total = 0;

        foreach (var id in achievementIds) {
            var achievement = GetUnlockableAchievement(id);
            total++;
            if (achievement.Unlocked()) obtained++;
        }

        return new PointsScore(obtained, total);
    }

    public static PointsScore CalculateAchievementPoints() {
        uint obtained = 0;
        uint total = 0;

        foreach (var achievement in ExcelSheets.Achievement.Value) {
            total += achievement.Points;
            if (Plugin.UnlockState.IsAchievementComplete(achievement)) {
                obtained += achievement.Points;
            }
        }

        return new PointsScore(obtained, total);
    }

    public bool GetAchievementListUpdatedForUi() {
        if (!achievementsWereLoaded && Plugin.UnlockState.IsAchievementListLoaded) {
            achievementsWereLoaded = true;
            return true;
        }

        if (unlocksUpdatedForUi) {
            unlocksUpdatedForUi = false;
            return true;
        }

        return false;
    }

    public void Refresh() {
        Log.Information("Refreshed unlockables");
        achievements.Clear();
        tieredAchievements.Clear();
        collectionItems.Clear();
    }
}
