using Recollection.Helpers;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using Recollection.External.Lalachievements;

namespace Recollection.Data.Unlockable;

public sealed record UnlockableFacewear : IUnlockable {
    private readonly GlassesStyle facewear;
    private readonly string name;
    private readonly string description;
    private readonly string nameLowercase;
    private readonly string descriptionLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    public UnlockableFacewear(GlassesStyle facewear, ITableRow tableRow) {
        this.facewear = facewear;
        name = facewear.Name.ToString();
        description = GetDescription(facewear);
        nameLowercase = facewear.Name.ToString().ToLower();
        descriptionLowercase = GetDescription(facewear).ToLower();
        howTo = tableRow.HowTo;
        howToLowercase = tableRow.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsGlassesStyleUnlocked(facewear);
    }

    public uint Id() => facewear.RowId;
    public UnlockableType Type() => UnlockableType.Facewear;
    public uint Icon() => (uint)facewear.Icon;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => facewear.IsValidEntry();

    private static string GetDescription(GlassesStyle style) {
        return style.Glasses.FirstOrNull()?.ValueNullable?.Description.ToString() ?? "";
    }
}
