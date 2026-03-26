using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Unlockables;

public class UnlockablesService
{
    private const string AchievementNameReplaceRegex = " ?[\\dIVX]+$";

    private readonly ExcelSheet<Achievement> achievementSheet = Plugin.DataManager.GetExcelSheet<Achievement>();

    private readonly ConcurrentDictionary<uint, UnlockableAchievement> achievements = new();
    private readonly ConcurrentDictionary<uint, UnlockableTieredAchievement> tieredAchievements = new();

    public UnlockableAchievement GetUnlockableAchievement(uint achievementId)
    {
        if (achievements.TryGetValue(achievementId, out var it))
        {
            return it;
        }

        var unlockable = new UnlockableAchievement(achievementSheet.GetRow(achievementId));
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
        var title = Regex.Replace(achievementList.Last().Name.ToString(), AchievementNameReplaceRegex, "");
        var unlockable = new UnlockableTieredAchievement(achievementList, title);
        tieredAchievements[achievementIds.Last()] = unlockable;
        return unlockable;
    }
}
