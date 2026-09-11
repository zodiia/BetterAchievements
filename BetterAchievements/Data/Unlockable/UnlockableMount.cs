using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableMount : IUnlockable {
    private readonly Mount mount;

    public UnlockableMount(Mount mount, ITable table) {
        this.mount = mount;
        name = mount.Singular.ToString();
        description = Plugin.DataManager.GetExcelSheet<MountTransient>().GetRow(mount.RowId).DescriptionEnhanced.ToString();
        nameLowercase = mount.Singular.ToString().ToLower();
        descriptionLowercase = Plugin.DataManager.GetExcelSheet<MountTransient>().GetRow(mount.RowId).DescriptionEnhanced.ToString().ToLower();
        howTo = table.HowTo;
        howToLowercase = table.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsMountUnlocked(mount);
    }

    public uint Id() => mount.RowId;
    public UnlockableType Type() => UnlockableType.Mount;
    public uint Icon() => mount.Icon;
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
}
