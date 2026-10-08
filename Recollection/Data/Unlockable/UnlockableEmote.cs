using MethodTimer;
using System;
using System.Linq;
using Recollection.Helpers;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Recollection.External.Lalachievements;
using Emote = Lumina.Excel.Sheets.Emote;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableEmote : IUnlockable {
    private readonly Emote emote;

    [Time]
    public UnlockableEmote(Emote emote, ITableRow tableRow) {
        this.emote = emote;
        Name = emote.Name.ToString();
        Description = GetDescription(emote.TextCommand);
        HowTo = tableRow.HowTo;
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description?.ToLower();
        HowToLowercase = HowTo?.ToLower();
        Unlocked = Plugin.UnlockState.IsEmoteUnlocked(emote);
    }

    public uint Id => emote.RowId;
    public UnlockableType Type => UnlockableType.Emote;
    public uint Icon => emote.Icon;

    public string Name { get; }
    public string? Description { get; }
    public string? HowTo { get; }
    public string NameLowercase { get; }
    public string? DescriptionLowercase { get; }
    public string? HowToLowercase { get; }

    public uint? Current {
        get => Unlocked ? 1u : 0u;
        set => throw new NotSupportedException("Current is derived from Unlocked.");
    }
    public uint Maximum => 1;
    public bool Unlocked { get; set; }

    public bool IsValid() => emote.IsValidEntry();

    private static string? GetDescription(RowRef<TextCommand> textCommand) {
        if (textCommand.ValueNullable is not { } command) return null;

        var aliases = new[] { command.Command, command.ShortCommand, command.Alias, command.ShortAlias }
                      .Select(it => it.ToString())
                      .Where(it => it.Length > 0)
                      .Distinct();

        var description = string.Join(", ", aliases);
        return description.Length > 0 ? description : null;
    }
}
