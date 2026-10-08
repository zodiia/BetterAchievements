using MethodTimer;
using System;
using Recollection.Helpers;
using Recollection.External.Lalachievements;
using TripleTriadCard = Lumina.Excel.Sheets.TripleTriadCard;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableTripleTriadCard : IUnlockable {
    private const uint CardIconOffset = 87000;
    private readonly TripleTriadCard card;

    [Time]
    public UnlockableTripleTriadCard(TripleTriadCard card, ITableRow tableRow) {
        this.card = card;
        Name = card.Name.ToString();
        Description = card.Description.ToString();
        HowTo = tableRow.HowTo;
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        HowToLowercase = HowTo?.ToLower();
        Unlocked = Plugin.UnlockState.IsTripleTriadCardUnlocked(card);
    }

    public uint Id => card.RowId;
    public UnlockableType Type => UnlockableType.TripleTriadCard;
    public uint Icon => CardIconOffset + card.RowId;

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

    public bool IsValid() => card.IsValidEntry();
}
