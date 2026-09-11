using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableBarding : IUnlockable {
    private readonly BuddyEquip barding;

    public UnlockableBarding(BuddyEquip barding, ITable table) {
        this.barding = barding;
        name = barding.Name.ToString();
        nameLowercase = barding.Name.ToString().ToLower();
        howTo = table.HowTo;
        howToLowercase = table.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsBuddyEquipUnlocked(barding);
    }

    public uint Id() => barding.RowId;
    public UnlockableType Type() => UnlockableType.Barding;
    public uint Icon() => barding.IconBody;
    private readonly string name;
    public string Name() => name;
    public string Description() => "";
    private readonly string nameLowercase;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => "";
    private readonly string? howTo;
    public string? HowTo() => howTo;
    private readonly string? howToLowercase;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked;
    public bool Unlocked() => unlocked;
}
