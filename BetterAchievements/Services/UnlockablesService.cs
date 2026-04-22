using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.Helpers;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Services;

public class UnlockablesService(Plugin plugin)
{
    private readonly ExcelSheet<Achievement> achievementSheet = Plugin.DataManager.GetExcelSheet<Achievement>();

    private readonly ConcurrentDictionary<uint, UnlockableAchievement> achievements = new();
    private readonly ConcurrentDictionary<uint, UnlockableTieredAchievement> tieredAchievements = new();

    public UnlockableAchievement GetUnlockableAchievement(uint achievementId)
    {
        if (achievements.TryGetValue(achievementId, out var it))
        {
            return it;
        }

        var unlockable = new UnlockableAchievement(achievementSheet.GetRow(achievementId), plugin);
        achievements[achievementId] = unlockable;
        return unlockable;
    }

    public UnlockableTieredAchievement GetUnlockableTieredAchievement(List<uint> achievementIds)
    {
        if (tieredAchievements.TryGetValue(achievementIds.Last(), out var it))
        {
            return it;
        }

        var achievementList = achievementIds.Select(id => achievementSheet.GetRow(id)).ToList();
        var title = CompiledRegexes.AchievementNameReplace().Replace(achievementList.Last().Name.ToString(), "");
        var unlockable = new UnlockableTieredAchievement(achievementList, title, plugin);
        tieredAchievements[achievementIds.Last()] = unlockable;
        return unlockable;
    }

    public IUnlockable? GetExistingAchievement(uint achievementId)
    {
        return achievements.GetValueOrDefault(achievementId) as IUnlockable ?? tieredAchievements.GetValueOrDefault(achievementId);
    }

    public void Refresh()
    {
        achievements.Clear();
        tieredAchievements.Clear();
    }
}
