using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableMount(Mount Mount) : IUnlockable {
    public uint Id() => Mount.RowId;
    public UnlockableType Type() => UnlockableType.Mount;
    public uint Icon() => Mount.Icon;
    private readonly string name = Mount.Singular.ToString();
    public string Name() => name;
    private readonly string description = Plugin.DataManager.GetExcelSheet<MountTransient>().GetRow(Mount.RowId).DescriptionEnhanced.ToString();
    public string Description() => description;
    private readonly string nameLowercase = Mount.Singular.ToString().ToLower();
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase = Plugin.DataManager.GetExcelSheet<MountTransient>().GetRow(Mount.RowId).DescriptionEnhanced.ToString().ToLower();
    public string DescriptionLowercase() => descriptionLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked = Plugin.UnlockState.IsMountUnlocked(Mount);
    public bool Unlocked() => unlocked;
}
