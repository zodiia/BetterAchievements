using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableBarding(BuddyEquip Barding, ITable Table) : IUnlockable {
    public uint Id() => Barding.RowId;
    public UnlockableType Type() => UnlockableType.Barding;
    public uint Icon() => Barding.IconBody;
    private readonly string name = Barding.Name.ToString();
    public string Name() => name;
    public string Description() => "";
    private readonly string nameLowercase = Barding.Name.ToString().ToLower();
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => "";
    private readonly string howTo = Table.PlainHowTo();
    public string HowTo() => howTo;
    private readonly string howToLowercase = Table.PlainHowTo().ToLower();
    public string HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked = Plugin.UnlockState.IsBuddyEquipUnlocked(Barding);
    public bool Unlocked() => unlocked;
}
