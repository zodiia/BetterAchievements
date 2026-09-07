using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace BetterAchievements.Services;

public class UnlockablesService {
    private readonly Plugin plugin;
    private readonly ExcelSheet<Achievement> achievementSheet = Plugin.DataManager.GetExcelSheet<Achievement>();

    private readonly ConcurrentDictionary<uint, UnlockableAchievement> achievements = new();
    private readonly ConcurrentDictionary<uint, UnlockableTieredAchievement> tieredAchievements = new();
    private readonly ConcurrentDictionary<UnlockableKey, IUnlockable> collectionItems = new();
    private bool achievementArrayUpdatedForUi = false;
    private bool achievementsWereLoaded = false;

    public readonly Dictionary<uint, uint> HighestIdMap;

    public UnlockablesService(Plugin plugin) {
        this.plugin = plugin;

        HighestIdMap = CalculateHighestIdMap(plugin.MainLayout);
        Plugin.UnlockState.Unlock += OnUnlock;
    }

    private void OnUnlock(RowRef _) {
        achievementArrayUpdatedForUi = true;
    }

    private static Dictionary<uint, uint> CalculateHighestIdMap(AchievementLayout layout) {
        var map = new Dictionary<uint, uint>();

        switch (layout) {
            case AchievementLayoutGroup group:
                foreach (var subLayout in group.Items) {
                    foreach (var (key, value) in CalculateHighestIdMap(subLayout)) {
                        map[key] = value;
                    }
                }

                break;

            case AchievementLayoutCategory category:
                foreach (var item in category.Items) {
                    switch (item) {
                        case AchievementLayoutItemSimple simple:
                            map[simple.Id] = simple.Id;
                            break;
                        case AchievementLayoutItemTiered tiered:
                            var lastId = tiered.Ids.Last();
                            foreach (var id in tiered.Ids) {
                                map[id] = lastId;
                            }

                            break;
                    }
                }

                break;
        }

        return map;
    }

    private static Dictionary<uint, uint> CalculateHighestIdMap(MainLayout mainLayout) {
        var map = new Dictionary<uint, uint>();

        foreach (var layout in mainLayout.Achievements) {
            foreach (var (key, value) in CalculateHighestIdMap(layout)) {
                map[key] = value;
            }
        }

        return map;
    }

    public UnlockableAchievement GetUnlockableAchievement(uint achievementId) {
        if (achievements.TryGetValue(achievementId, out var it)) {
            return it;
        }

        var unlockable = new UnlockableAchievement(achievementSheet.GetRow(achievementId), plugin);
        achievements[achievementId] = unlockable;
        return unlockable;
    }

    public UnlockableTieredAchievement GetUnlockableTieredAchievement(List<uint> achievementIds, bool spoilers) {
        if (tieredAchievements.TryGetValue(achievementIds.Last(), out var it)) {
            return it;
        }

        var achievementList = achievementIds.Select(id => achievementSheet.GetRow(id)).ToList();
        var unlockable = new UnlockableTieredAchievement(achievementList, spoilers, plugin);
        achievementIds.ForEach(id => tieredAchievements[id] = unlockable);
        return unlockable;
    }

    private static IUnlockable CreateUnlockableCollectionItem(UnlockableType type, uint id) {
        return type switch {
            UnlockableType.Mount => new UnlockableMount(Plugin.DataManager.GetExcelSheet<Mount>().GetRow(id)),
            UnlockableType.Minion => new UnlockableMinion(Plugin.DataManager.GetExcelSheet<Companion>().GetRow(id)),
            UnlockableType.Title => new UnlockableTitle(Plugin.DataManager.GetExcelSheet<Title>().GetRow(id)),
            UnlockableType.TripleTriadCard => new UnlockableTripleTriadCard(Plugin.DataManager.GetExcelSheet<TripleTriadCard>().GetRow(id)),
            UnlockableType.Barding => new UnlockableBarding(Plugin.DataManager.GetExcelSheet<BuddyEquip>().GetRow(id)),
            UnlockableType.FashionAccessory => new UnlockableFashionAccessory(Plugin.DataManager.GetExcelSheet<Ornament>().GetRow(id)),
            UnlockableType.Hairstyle => new UnlockableHairstyle(Plugin.DataManager.GetExcelSheet<CharaMakeCustomize>().First(it => it.FeatureID == id)),
            UnlockableType.Facewear => new UnlockableFacewear(Plugin.DataManager.GetExcelSheet<GlassesStyle>().GetRow(id)),
            UnlockableType.Emote => new UnlockableEmote(Plugin.DataManager.GetExcelSheet<Emote>().GetRow(id)),
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

    public IUnlockable GetUnlockableCollectionItem(UnlockableType type, uint id) {
        var key = new UnlockableKey(type, id);
        if (collectionItems.TryGetValue(key, out var it)) {
            return it;
        }

        var unlockable = CreateUnlockableCollectionItem(type, id);
        collectionItems[key] = unlockable;
        return unlockable;
    }

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

        if (achievementArrayUpdatedForUi) {
            achievementArrayUpdatedForUi = false;
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
