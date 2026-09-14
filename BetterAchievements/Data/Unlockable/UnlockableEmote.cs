using System.Linq;
using BetterAchievements.External.Lalachievements;
using BetterAchievements.Helpers;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Emote = Lumina.Excel.Sheets.Emote;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableEmote : IUnlockable {
    private readonly Emote emote;
    private readonly string name;
    private readonly string description;
    private readonly string nameLowercase;
    private readonly string descriptionLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    public UnlockableEmote(Emote emote, ITableRow tableRow) {
        this.emote = emote;
        name = emote.Name.ToString();
        description = DisplayDescription(emote.TextCommand);
        nameLowercase = emote.Name.ToString().ToLower();
        descriptionLowercase = DisplayDescription(emote.TextCommand).ToLower();
        howTo = tableRow.HowTo;
        howToLowercase = tableRow.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsEmoteUnlocked(emote);
    }

    public uint Id() => emote.RowId;
    public UnlockableType Type() => UnlockableType.Emote;
    public uint Icon() => emote.Icon;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => emote.IsValidEntry();

    private static string DisplayDescription(RowRef<TextCommand> textCommand) {
        if (textCommand.ValueNullable is not { } command) return "";

        var aliases = new[] { command.Command, command.ShortCommand, command.Alias, command.ShortAlias }
                      .Select(it => it.ToString())
                      .Where(it => it.Length > 0)
                      .Distinct();

        return string.Join(", ", aliases);
    }
}
