using MethodTimer;
using System;
using Lumina.Excel.Sheets;
using Recollection.External.Lalachievements;
using Recollection.Helpers;
using Mount = Lumina.Excel.Sheets.Mount;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableMount : IUnlockable {
    private readonly Mount mount;

    [Time]
    public UnlockableMount(Mount mount, ITableRow tableRow) {
        this.mount = mount;
        Name = mount.Singular.ToString();
        Description = ExcelSheets.MountTransient.Value.GetRow(mount.RowId).DescriptionEnhanced.ToString();
        HowTo = tableRow.HowTo;
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        HowToLowercase = HowTo?.ToLower();
        Unlocked = Plugin.UnlockState.IsMountUnlocked(mount);
    }

    public uint Id => mount.RowId;
    public UnlockableType Type => UnlockableType.Mount;
    public uint Icon => mount.Icon;

    public string Name { get; }
    public string? Description { get; }
    public string? HowTo { get; }
    public string NameLowercase { get; }
    public string? DescriptionLowercase { get; }
    public string? HowToLowercase { get; }

    public uint? Current {
        get => Unlocked ? 1u : 0u;
        set => throw new NotSupportedException("Current is derived from Unlocked.");
    }
    public uint Maximum => 1;
    public bool Unlocked { get; set; }

    public bool IsValid() => mount.IsValidEntry();
}
