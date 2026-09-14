using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using LalachievementsService = BetterAchievements.External.Lalachievements.LalachievementsService;
using ITableRow = BetterAchievements.External.Lalachievements.ITableRow;

namespace BetterAchievements.Services;

public class UnlockablesService {
    private readonly Plugin plugin;
    private readonly LalachievementsService lalachievementsService;
    private readonly ConcurrentDictionary<uint, UnlockableAchievement> achievements = new();
    private readonly ConcurrentDictionary<uint, UnlockableTieredAchievement> tieredAchievements = new();
    private readonly ConcurrentDictionary<UnlockableKey, IUnlockable> collectionItems = new();
    private readonly ConcurrentDictionary<UnlockableType, object> excelSheetCache = new();
    public readonly Dictionary<uint, uint> HighestAchievementIdMap;
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

    private ExcelSheet<T> GetExcelSheet<T>(UnlockableType type) where T : struct, IExcelRow<T> {
        return (ExcelSheet<T>)excelSheetCache.GetOrAdd(type, static _ => Plugin.DataManager.GetExcelSheet<T>());
    }

    private ITableRow GetTableRow<T>(uint id) where T : ITableRow {
        return lalachievementsService.GetTable<T>().First(it => it.Id == id);
    }

    public UnlockableAchievement GetUnlockableAchievement(uint achievementId) {
        if (achievements.TryGetValue(achievementId, out var it)) {
            return it;
        }

        var unlockable = new UnlockableAchievement(GetExcelSheet<Achievement>(UnlockableType.Achievement).GetRow(achievementId), plugin);
        achievements[achievementId] = unlockable;
        return unlockable;
    }

    public UnlockableTieredAchievement GetUnlockableTieredAchievement(List<uint> achievementIds, bool spoilers) {
        if (tieredAchievements.TryGetValue(achievementIds.Last(), out var it)) {
            return it;
        }

        var achievementList = achievementIds.Select(id => GetExcelSheet<Achievement>(UnlockableType.Achievement).GetRow(id)).ToList();
        var unlockable = new UnlockableTieredAchievement(achievementList, spoilers, plugin);
        achievementIds.ForEach(id => tieredAchievements[id] = unlockable);
        return unlockable;
    }

    private static IUnlockable CreateUnlockableCollectionItem(UnlockableType type, ITable row) {
        var id = row.Id;

        return type switch {
            UnlockableType.Mount => new UnlockableMount(Plugin.DataManager.GetExcelSheet<Mount>().GetRow(id), row),
            UnlockableType.Minion => new UnlockableMinion(Plugin.DataManager.GetExcelSheet<Companion>().GetRow(id), row),
            UnlockableType.Title => new UnlockableTitle(Plugin.DataManager.GetExcelSheet<Title>().GetRow(id), row),
            UnlockableType.TripleTriadCard => new UnlockableTripleTriadCard(Plugin.DataManager.GetExcelSheet<TripleTriadCard>().GetRow(id), row),
            UnlockableType.Barding => new UnlockableBarding(Plugin.DataManager.GetExcelSheet<BuddyEquip>().GetRow(id), row),
            UnlockableType.FashionAccessory => new UnlockableFashionAccessory(Plugin.DataManager.GetExcelSheet<Ornament>().GetRow(id), row),
            UnlockableType.Hairstyle => new UnlockableHairstyle(Plugin.DataManager.GetExcelSheet<CharaMakeCustomize>().First(it => it.FeatureID == id), row),
            UnlockableType.Facewear => new UnlockableFacewear(Plugin.DataManager.GetExcelSheet<GlassesStyle>().GetRow(id), row),
            UnlockableType.Emote => new UnlockableEmote(Plugin.DataManager.GetExcelSheet<Emote>().GetRow(id), row),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static bool IsValidCollectionItem(UnlockableType type, uint id) {
        return type switch {
            UnlockableType.Mount =>
                Plugin.DataManager.GetExcelSheet<Mount>().GetRowOrDefault(id) is { Order: > -1, Singular.IsEmpty: false },
            UnlockableType.Minion =>
                Plugin.DataManager.GetExcelSheet<Companion>().GetRowOrDefault(id) is { Singular.IsEmpty: false },
            UnlockableType.Title =>
                Plugin.DataManager.GetExcelSheet<Title>().GetRowOrDefault(id) is { Masculine.IsEmpty: false },
            UnlockableType.TripleTriadCard =>
                Plugin.DataManager.GetExcelSheet<TripleTriadCard>().GetRowOrDefault(id) is { Name.IsEmpty: false },
            UnlockableType.Barding => Plugin.DataManager.GetExcelSheet<BuddyEquip>().GetRowOrDefault(id) is { Order: > 0, Name.IsEmpty: false },
            UnlockableType.FashionAccessory =>
                Plugin.DataManager.GetExcelSheet<Ornament>().GetRowOrDefault(id) is { Singular.IsEmpty: false },
            UnlockableType.Hairstyle =>
                Plugin.DataManager.GetExcelSheet<CharaMakeCustomize>().TryGetFirst(it => it.FeatureID == id, out var hairstyle)
                && hairstyle is { IsPurchasable: true, RowId: < 2400, FeatureID: not 130, FeatureID: not 159 },
            UnlockableType.Facewear =>
                Plugin.DataManager.GetExcelSheet<GlassesStyle>().GetRowOrDefault(id) is { Name.IsEmpty: false } facewear
                && facewear.Glasses.FirstOrNull()?.IsValid == true && !facewear.Glasses.FirstOrNull()?.Value.Name.IsEmpty == true,
            UnlockableType.Emote =>
                Plugin.DataManager.GetExcelSheet<Emote>().GetRowOrDefault(id) is { Order: > 0, Name.IsEmpty: false },
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    private IUnlockable CreateUnlockable(UnlockableKey key) => key.Type switch {
        UnlockableType.Mount =>
            new UnlockableMount(GetExcelSheet<Mount>(key.Type).GetRow(key.Id),
                                GetTableRow<External.Lalachievements.Mount>(key.Id)),
        UnlockableType.Minion =>
            new UnlockableMinion(GetExcelSheet<Companion>(key.Type).GetRow(key.Id),
                                 GetTableRow<External.Lalachievements.Minion>(key.Id)),
        UnlockableType.Title =>
            new UnlockableTitle(GetExcelSheet<Title>(key.Type).GetRow(key.Id),
                                GetTableRow<External.Lalachievements.Title>(key.Id)),
        UnlockableType.TripleTriadCard =>
            new UnlockableTripleTriadCard(GetExcelSheet<TripleTriadCard>(key.Type).GetRow(key.Id),
                                          GetTableRow<External.Lalachievements.TripleTriadCard>(key.Id)),
        UnlockableType.Barding =>
            new UnlockableBarding(GetExcelSheet<BuddyEquip>(key.Type).GetRow(key.Id),
                                  GetTableRow<External.Lalachievements.Barding>(key.Id)),
        UnlockableType.FashionAccessory =>
            new UnlockableFashionAccessory(GetExcelSheet<Ornament>(key.Type).GetRow(key.Id),
                                           GetTableRow<External.Lalachievements.Fashion>(key.Id)),
        UnlockableType.Hairstyle =>
            new UnlockableHairstyle(GetExcelSheet<CharaMakeCustomize>(key.Type).First(it => it.FeatureID == key.Id),
                                    GetTableRow<External.Lalachievements.Hair>(key.Id)),
        UnlockableType.Facewear =>
            new UnlockableFacewear(GetExcelSheet<GlassesStyle>(key.Type).GetRow(key.Id),
                                   GetTableRow<External.Lalachievements.Spectacle>(key.Id)),
        UnlockableType.Emote =>
            new UnlockableEmote(GetExcelSheet<Emote>(key.Type).GetRow(key.Id),
                                GetTableRow<External.Lalachievements.Emote>(key.Id)),
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

        foreach (var achievement in Plugin.DataManager.GetExcelSheet<Achievement>()) {
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
        achievements.Clear();
        tieredAchievements.Clear();
        collectionItems.Clear();
    }
}
