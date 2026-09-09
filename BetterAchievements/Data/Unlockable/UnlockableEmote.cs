using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableEmote(Emote Emote, ITable Table) : IUnlockable {
    public uint Id() => Emote.RowId;
    public UnlockableType Type() => UnlockableType.Emote;
    public uint Icon() => Emote.Icon;
    private readonly string name = Emote.Name.ToString();
    public string Name() => name;
    private readonly string description = DisplayDescription(Emote.TextCommand);
    public string Description() => description;
    private readonly string nameLowercase = Emote.Name.ToString().ToLower();
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase = DisplayDescription(Emote.TextCommand).ToLower();
    public string DescriptionLowercase() => descriptionLowercase;
    private readonly string howTo = Table.PlainHowTo();
    public string HowTo() => howTo;
    private readonly string howToLowercase = Table.PlainHowTo().ToLower();
    public string HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked = Plugin.UnlockState.IsEmoteUnlocked(Emote);
    public bool Unlocked() => unlocked;

    private static string DisplayDescription(RowRef<TextCommand> textCommand) {
        if (textCommand.ValueNullable is not { } command) return "";

        var aliases = new[] { command.Command, command.ShortCommand, command.Alias, command.ShortAlias }
                      .Select(it => it.ToString())
                      .Where(it => it.Length > 0)
                      .Distinct();

        return string.Join(", ", aliases);
    }
}
