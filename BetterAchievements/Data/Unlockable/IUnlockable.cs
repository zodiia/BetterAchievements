namespace BetterAchievements.Data.Unlockable;

public enum UnlockableType {
    Achievement,
    Mount,
    Minion,
    Title,
    TripleTriadCard,
    Barding,
    FashionAccessory,
    Hairstyle,
    Facewear,
    Emote,
}

public interface IUnlockable {
    uint Id();
    UnlockableType Type();
    uint Icon();
    string Name();
    string Description();
    string NameLowercase();
    string DescriptionLowercase();
    uint? Current();
    uint Maximum();
    bool Unlocked();
}
