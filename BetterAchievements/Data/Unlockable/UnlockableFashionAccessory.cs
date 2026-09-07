using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableFashionAccessory(Ornament Ornament) : IUnlockable {
    public uint Id() => Ornament.RowId;
    public UnlockableType Type() => UnlockableType.FashionAccessory;
    public uint Icon() => Ornament.Icon;
    private readonly string name = Ornament.Singular.ToString();
    public string Name() => name;
    private readonly string description = DisplayDescription(Ornament);
    public string Description() => description;
    private readonly string nameLowercase = Ornament.Singular.ToString().ToLower();
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase = DisplayDescription(Ornament).ToLower();
    public string DescriptionLowercase() => descriptionLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked = Plugin.UnlockState.IsOrnamentUnlocked(Ornament);
    public bool Unlocked() => unlocked;

    private static string DisplayDescription(Ornament ornament) {
        return Plugin.DataManager.GetExcelSheet<OrnamentTransient>().GetRowOrDefault(ornament.RowId)?.Text.ToString() ?? "";
    }
}
