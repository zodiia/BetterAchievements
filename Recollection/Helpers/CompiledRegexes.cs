using System.Text.RegularExpressions;

namespace Recollection.Helpers;

public static partial class CompiledRegexes {
    [GeneratedRegex(@" ?[\\dIVX]+$", RegexOptions.Compiled)]
    public static partial Regex AchievementNameReplace();

    [GeneratedRegex(@"<.*?>(\\n)?", RegexOptions.Compiled)]
    public static partial Regex HtmlTagStrip();
}
