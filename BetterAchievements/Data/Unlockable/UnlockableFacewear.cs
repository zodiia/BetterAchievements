using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableFacewear(GlassesStyle Facewear) : IUnlockable {
    public uint Id() => Facewear.RowId;
    public UnlockableType Type() => UnlockableType.Facewear;
    public uint Icon() => (uint)Facewear.Icon;
    private readonly string name = Facewear.Name.ToString();
    public string Name() => name;
    private readonly string description = DisplayDescription(Facewear);
    public string Description() => description;
    private readonly string nameLowercase = Facewear.Name.ToString().ToLower();
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase = DisplayDescription(Facewear).ToLower();
    public string DescriptionLowercase() => descriptionLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked = Plugin.UnlockState.IsGlassesStyleUnlocked(Facewear);
    public bool Unlocked() => unlocked;

    private static string DisplayDescription(GlassesStyle style) {
        return style.Glasses.FirstOrNull()?.ValueNullable?.Description.ToString() ?? "";
    }
}
