using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Services;

public class UnlockablesService(Plugin plugin) {
    private readonly ExcelSheet<Achievement> achievementSheet = Plugin.DataManager.GetExcelSheet<Achievement>();

    private readonly ConcurrentDictionary<uint, UnlockableAchievement> achievements = new();
    private readonly ConcurrentDictionary<uint, UnlockableTieredAchievement> tieredAchievements = new();

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

    public void Refresh() {
        achievements.Clear();
        tieredAchievements.Clear();
    }
}
