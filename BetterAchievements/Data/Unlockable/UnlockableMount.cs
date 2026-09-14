using BetterAchievements.External.Lalachievements;
using BetterAchievements.Helpers;
using Lumina.Excel.Sheets;
using Mount = Lumina.Excel.Sheets.Mount;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableMount : IUnlockable {
    private readonly Mount mount;
    private readonly string name;
    private readonly string description;
    private readonly string nameLowercase;
    private readonly string descriptionLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    public UnlockableMount(Mount mount, ITableRow tableRow) {
        this.mount = mount;
        name = mount.Singular.ToString();
        description = Plugin.DataManager.GetExcelSheet<MountTransient>().GetRow(mount.RowId).DescriptionEnhanced.ToString();
        nameLowercase = mount.Singular.ToString().ToLower();
        descriptionLowercase = Plugin.DataManager.GetExcelSheet<MountTransient>().GetRow(mount.RowId).DescriptionEnhanced.ToString().ToLower();
        howTo = tableRow.HowTo;
        howToLowercase = tableRow.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsMountUnlocked(mount);
    }

    public uint Id() => mount.RowId;
    public UnlockableType Type() => UnlockableType.Mount;
    public uint Icon() => mount.Icon;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => mount.IsValidEntry();
}
