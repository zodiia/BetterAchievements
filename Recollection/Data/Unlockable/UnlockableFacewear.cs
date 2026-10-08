using MethodTimer;
using System;
using Recollection.Helpers;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using Recollection.External.Lalachievements;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableFacewear : IUnlockable {
    private readonly GlassesStyle facewear;

    [Time]
    public UnlockableFacewear(GlassesStyle facewear, ITableRow tableRow) {
        this.facewear = facewear;
        Name = facewear.Name.ToString();
        Description = GetDescription(facewear);
        HowTo = tableRow.HowTo;
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description?.ToLower();
        HowToLowercase = HowTo?.ToLower();
        Unlocked = Plugin.UnlockState.IsGlassesStyleUnlocked(facewear);
    }

    public uint Id => facewear.RowId;
    public UnlockableType Type => UnlockableType.Facewear;
    public uint Icon => (uint)facewear.Icon;

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

    public bool IsValid() => facewear.IsValidEntry();

    private static string? GetDescription(GlassesStyle style) {
        return style.Glasses.FirstOrNull()?.ValueNullable?.Description.ToString();
    }
}
