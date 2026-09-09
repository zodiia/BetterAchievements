using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableTripleTriadCard(TripleTriadCard Card, ITable Table) : IUnlockable {
    private const uint CardIconOffset = 87000;

    public uint Id() => Card.RowId;
    public UnlockableType Type() => UnlockableType.TripleTriadCard;
    public uint Icon() => CardIconOffset + Card.RowId;
    private readonly string name = Card.Name.ToString();
    public string Name() => name;
    private readonly string description = Card.Description.ToString();
    public string Description() => description;
    private readonly string nameLowercase = Card.Name.ToString().ToLower();
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase = Card.Description.ToString().ToLower();
    public string DescriptionLowercase() => descriptionLowercase;
    private readonly string howTo = Table.PlainHowTo();
    public string HowTo() => howTo;
    private readonly string howToLowercase = Table.PlainHowTo().ToLower();
    public string HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked = Plugin.UnlockState.IsTripleTriadCardUnlocked(Card);
    public bool Unlocked() => unlocked;
}
