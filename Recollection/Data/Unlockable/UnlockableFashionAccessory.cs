using MethodTimer;
using System;
using Lumina.Excel.Sheets;
using Recollection.External.Lalachievements;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableFashionAccessory : IUnlockable {
    private readonly Ornament ornament;

    [Time]
    public UnlockableFashionAccessory(Ornament ornament, ITableRow tableRow) {
        this.ornament = ornament;
        Name = ornament.Singular.ToString();
        Description = GetDescription(ornament);
        HowTo = tableRow.HowTo;
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description?.ToLower();
        HowToLowercase = HowTo?.ToLower();
        Unlocked = Plugin.UnlockState.IsOrnamentUnlocked(ornament);
    }

    public uint Id => ornament.RowId;
    public UnlockableType Type => UnlockableType.FashionAccessory;
    public uint Icon => ornament.Icon;

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

    public bool IsValid() => ornament.IsValidEntry();

    private static string? GetDescription(Ornament ornament) {
        return ExcelSheets.OrnamentTransient.Value.GetRowOrDefault(ornament.RowId)?.Text.ToString();
    }
}
