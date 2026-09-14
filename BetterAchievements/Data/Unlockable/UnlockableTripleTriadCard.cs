using BetterAchievements.External.Lalachievements;
using BetterAchievements.Helpers;
using TripleTriadCard = Lumina.Excel.Sheets.TripleTriadCard;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableTripleTriadCard : IUnlockable {
    private const uint CardIconOffset = 87000;
    private readonly TripleTriadCard card;

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
    public bool IsValid() => card.IsValidEntry();
}
