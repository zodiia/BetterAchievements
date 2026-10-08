using MethodTimer;
using System;
using Recollection.Helpers;
using Lumina.Excel.Sheets;
using Recollection.External.Lalachievements;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableHairstyle : IUnlockable {
    private readonly CharaMakeCustomize hairstyle;

    [Time]
    public UnlockableHairstyle(CharaMakeCustomize hairstyle, ITableRow tableRow) {
        this.hairstyle = hairstyle;
        Name = hairstyle.HintItem.ValueNullable?.Name.ToString() ?? "";
        Description = hairstyle.HintItem.ValueNullable?.Description.ToString();
        HowTo = tableRow.HowTo;
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description?.ToLower();
        HowToLowercase = HowTo?.ToLower();
        Unlocked = Plugin.UnlockState.IsCharaMakeCustomizeUnlocked(hairstyle);
    }

    public uint Id => hairstyle.FeatureID;
    public UnlockableType Type => UnlockableType.Hairstyle;
    public uint Icon => hairstyle.Icon;

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

    public bool IsValid() => hairstyle.IsValidEntry();
}
