using MethodTimer;
using Recollection.Helpers;
using Recollection.External.Lalachievements;
using TripleTriadCard = Lumina.Excel.Sheets.TripleTriadCard;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableTripleTriadCard : IUnlockable {
    private const uint CardIconOffset = 87000;
    private readonly TripleTriadCard card;
    private readonly string name;
    private readonly string nameLowercase;
    private readonly string description;
    private readonly string descriptionLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    [Time]
    public UnlockableTripleTriadCard(TripleTriadCard card, ITableRow tableRow) {
        this.card = card;
        name = card.Name.ToString();
        description = card.Description.ToString();
        howTo = tableRow.HowTo;
        nameLowercase = name.ToLower();
        descriptionLowercase = description.ToLower();
        howToLowercase = howTo?.ToLower();
        unlocked = Plugin.UnlockState.IsTripleTriadCardUnlocked(card);
    }

    public uint Id() => card.RowId;
    public UnlockableType Type() => UnlockableType.TripleTriadCard;
    public uint Icon() => CardIconOffset + card.RowId;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => card.IsValidEntry();
}
