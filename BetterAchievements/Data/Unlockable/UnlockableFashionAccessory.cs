using BetterAchievements.External.Lalachievements;
using BetterAchievements.Helpers;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableFashionAccessory : IUnlockable {
    private readonly Ornament ornament;
    private readonly string name;
    private readonly string description;
    private readonly string nameLowercase;
    private readonly string descriptionLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    public UnlockableFashionAccessory(Ornament ornament, ITableRow tableRow) {
        this.ornament = ornament;
        name = ornament.Singular.ToString();
        description = GetDescription(ornament);
        nameLowercase = ornament.Singular.ToString().ToLower();
        descriptionLowercase = GetDescription(ornament).ToLower();
        howTo = tableRow.HowTo;
        howToLowercase = tableRow.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsOrnamentUnlocked(ornament);
    }

    public uint Id() => ornament.RowId;
    public UnlockableType Type() => UnlockableType.FashionAccessory;
    public uint Icon() => ornament.Icon;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => ornament.IsValidEntry();

    private static string GetDescription(Ornament ornament) {
        return ExcelSheets.OrnamentTransient.Value.GetRowOrDefault(ornament.RowId)?.Text.ToString() ?? "";
    }
}
