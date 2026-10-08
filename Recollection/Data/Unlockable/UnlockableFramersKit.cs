using MethodTimer;
using System;
using Lumina.Excel.Sheets;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableFramersKit : IUnlockable {
    private readonly Item item;

    [Time]
    public UnlockableFramersKit(Item item) {
        this.item = item;
        Name = item.Name.ToString();
        Description = item.Description.ToString();
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        Unlocked = Plugin.UnlockState.IsItemUnlocked(item);
    }

    public uint Id => item.RowId;
    public UnlockableType Type => UnlockableType.FramersKit;
    public uint Icon => item.Icon;

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

    public bool IsValid() => item.IsValidEntry();
}
