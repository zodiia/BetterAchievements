using MethodTimer;
using System;
using Recollection.Helpers;
using Lumina.Excel.Sheets;
using Recollection.External.Lalachievements;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableBarding : IUnlockable {
    private readonly BuddyEquip barding;

    [Time]
    public UnlockableBarding(BuddyEquip barding, ITableRow tableRow) {
        this.barding = barding;
        Name = barding.Name.ToString();
        HowTo = tableRow.HowTo;
        NameLowercase = Name.ToLower();
        HowToLowercase = HowTo?.ToLower();
        Unlocked = Plugin.UnlockState.IsBuddyEquipUnlocked(barding);
    }

    public uint Id => barding.RowId;
    public UnlockableType Type => UnlockableType.Barding;
    public uint Icon => barding.IconBody;

    public string Name { get; }
    public string? Description => null;
    public string? HowTo { get; }
    public string NameLowercase { get; }
    public string? DescriptionLowercase => null;
    public string? HowToLowercase { get; }

    public uint? Current {
        get => Unlocked ? 1u : 0u;
        set => throw new NotSupportedException("Current is derived from Unlocked.");
    }
    public uint Maximum => 1;
    public bool Unlocked { get; set; }

    public bool IsValid() => barding.IsValidEntry();
}
