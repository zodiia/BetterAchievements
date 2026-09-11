using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableEmote : IUnlockable {
    private readonly Emote emote;

    public UnlockableEmote(Emote emote, ITable table) {
        this.emote = emote;
        name = emote.Name.ToString();
        description = DisplayDescription(emote.TextCommand);
        nameLowercase = emote.Name.ToString().ToLower();
        descriptionLowercase = DisplayDescription(emote.TextCommand).ToLower();
        howTo = table.HowTo;
        howToLowercase = table.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsEmoteUnlocked(emote);
    }

    public uint Id() => emote.RowId;
    public UnlockableType Type() => UnlockableType.Emote;
    public uint Icon() => emote.Icon;
    private readonly string name;
    public string Name() => name;
    private readonly string description;
    public string Description() => description;
    private readonly string nameLowercase;
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    private readonly string? howTo;
    public string? HowTo() => howTo;
    private readonly string? howToLowercase;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked;
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
