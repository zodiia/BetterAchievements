using System.Text.RegularExpressions;

namespace BetterAchievements.Helpers;

public static partial class CompiledRegexes
{
    [GeneratedRegex(@" ?[\\dIVX]+$", RegexOptions.Compiled)]
    public static partial Regex AchievementNameReplace();
}
