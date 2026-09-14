namespace BetterAchievements.Data.Unlockable;

public enum UnlockableType {
    Achievement,
    Mount,
    Minion,
    Title,
    TripleTriadCard,
    TripleTriadNpc,
    Barding,
    FashionAccessory,
    Hairstyle,
    Facewear,
    Emote,
    Fish,
    Spearfish,
    FramersKit,
    HuntingLog,
    CraftingLog,
    GatheringLog,
    OrchestrionRoll,
    AetherCurrent,
    FieldRecord,
    OccultRecord,
    SurveyRecord,
}

public interface IUnlockable {
    uint Id();
    UnlockableType Type();
    uint Icon();
    string Name();
    string Description();
    string? HowTo();
    string NameLowercase();
    string DescriptionLowercase();
    string? HowToLowercase();
    uint? Current();
    uint Maximum();
    bool Unlocked();
    bool IsValid();
}
