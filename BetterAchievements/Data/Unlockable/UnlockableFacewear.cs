using Lumina.Excel.Sheets;
using Lumina.Extensions;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableFacewear : IUnlockable {
    private readonly GlassesStyle facewear;

    public UnlockableFacewear(GlassesStyle facewear, ITable table) {
        this.facewear = facewear;
        name = facewear.Name.ToString();
        description = DisplayDescription(facewear);
        nameLowercase = facewear.Name.ToString().ToLower();
        descriptionLowercase = DisplayDescription(facewear).ToLower();
        howTo = table.HowTo;
        howToLowercase = table.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsGlassesStyleUnlocked(facewear);
    }

    public uint Id() => facewear.RowId;
    public UnlockableType Type() => UnlockableType.Facewear;
    public uint Icon() => (uint)facewear.Icon;
    private readonly string name;
    public string Name() => name;
    private readonly string description;
    public string Description() => description;
    private readonly string nameLowercase;
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    private readonly string? howTo;
    public string? HowTo() => howTo;
    private readonly string? howToLowercase;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked;
    public bool Unlocked() => unlocked;

    private static string DisplayDescription(GlassesStyle style) {
        return style.Glasses.FirstOrNull()?.ValueNullable?.Description.ToString() ?? "";
    }
}
