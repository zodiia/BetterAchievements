using BetterAchievements.External.Lalachievements;
using BetterAchievements.Helpers;
using TripleTriadCard = Lumina.Excel.Sheets.TripleTriadCard;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableTripleTriadCard : IUnlockable {
    private const uint CardIconOffset = 87000;
    private readonly TripleTriadCard card;
    private readonly string name;
    private readonly string description;
    private readonly string? howTo;
    private readonly string nameLowercase;
    private readonly string descriptionLowercase;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    public UnlockableTripleTriadCard(TripleTriadCard card, ITableRow tableRow) {
        this.card = card;
        name = card.Name.ToString();
        description = card.Description.ToString();
        nameLowercase = card.Name.ToString().ToLower();
        descriptionLowercase = card.Description.ToString().ToLower();
        howTo = tableRow.HowTo;
        howToLowercase = tableRow.HowTo?.ToLower();
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
