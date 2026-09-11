using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableFashionAccessory : IUnlockable {
    private readonly Ornament ornament;

    public UnlockableFashionAccessory(Ornament ornament, ITable table) {
        this.ornament = ornament;
        name = ornament.Singular.ToString();
        description = DisplayDescription(ornament);
        nameLowercase = ornament.Singular.ToString().ToLower();
        descriptionLowercase = DisplayDescription(ornament).ToLower();
        howTo = table.HowTo;
        howToLowercase = table.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsOrnamentUnlocked(ornament);
    }

    public uint Id() => ornament.RowId;
    public UnlockableType Type() => UnlockableType.FashionAccessory;
    public uint Icon() => ornament.Icon;
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

    private static string DisplayDescription(Ornament ornament) {
        return Plugin.DataManager.GetExcelSheet<OrnamentTransient>().GetRowOrDefault(ornament.RowId)?.Text.ToString() ?? "";
    }
}
