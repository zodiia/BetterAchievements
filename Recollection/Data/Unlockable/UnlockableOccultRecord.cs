using MethodTimer;
using System;
using Lumina.Excel.Sheets;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableOccultRecord : IUnlockable {
    private readonly MKDLore lore;

    [Time]
    public UnlockableOccultRecord(MKDLore lore) {
        this.lore = lore;
        Name = lore.Name.ToString();
        Description = lore.Description.ToString();
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        Unlocked = Plugin.UnlockState.IsMKDLoreUnlocked(lore);
    }

    public uint Id => lore.RowId;
    public UnlockableType Type => UnlockableType.OccultRecord;
    public uint Icon => lore.Image;

    public string Name { get; }
    public string Description { get; }
    public string? HowTo => null;
    public string NameLowercase { get; }
    public string DescriptionLowercase { get; }
    public string? HowToLowercase => null;

    public uint? Current {
        get => Unlocked ? 1u : 0u;
        set => throw new NotSupportedException("Current is derived from Unlocked.");
    }
    public uint Maximum => 1;
    public bool Unlocked { get; set; }

    public bool IsValid() => lore.IsValidEntry();
}
