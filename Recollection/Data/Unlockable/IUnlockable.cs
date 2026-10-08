namespace Recollection.Data.Unlockable;

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
    Leve,
}

public interface IUnlockable {
    uint Id { get; }
    UnlockableType Type { get; }
    uint Icon { get; }
    string Name { get; }
    string? Description { get; }
    string? HowTo { get; }
    string NameLowercase { get; }
    string? DescriptionLowercase { get; }
    string? HowToLowercase { get; }
    uint? Current { get; set; }
    uint Maximum { get; }
    bool Unlocked { get; set; }
    bool IsValid();
}
