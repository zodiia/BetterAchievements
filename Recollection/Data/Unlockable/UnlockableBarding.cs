using MethodTimer;
using Recollection.Helpers;
using Lumina.Excel.Sheets;
using Recollection.External.Lalachievements;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableBarding : IUnlockable {
    private readonly BuddyEquip barding;
    private readonly string name;
    private readonly string nameLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    [Time]
    public UnlockableBarding(BuddyEquip barding, ITableRow tableRow) {
        this.barding = barding;
        name = barding.Name.ToString();
        howTo = tableRow.HowTo;
        nameLowercase = name.ToLower();
        howToLowercase = howTo?.ToLower();
        unlocked = Plugin.UnlockState.IsBuddyEquipUnlocked(barding);
    }

    public uint Id() => barding.RowId;
    public UnlockableType Type() => UnlockableType.Barding;
    public uint Icon() => barding.IconBody;
    public string Name() => name;
    public string Description() => "";
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => "";
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => barding.IsValidEntry();

}
