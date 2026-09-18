using Recollection.Helpers;
using Lumina.Excel.Sheets;
using Recollection.External.Lalachievements;

namespace Recollection.Data.Unlockable;

public sealed record UnlockableHairstyle : IUnlockable {
    private readonly CharaMakeCustomize hairstyle;
    private readonly string name;
    private readonly string description;
    private readonly string nameLowercase;
    private readonly string descriptionLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    public UnlockableHairstyle(CharaMakeCustomize hairstyle, ITableRow tableRow) {
        this.hairstyle = hairstyle;
        name = hairstyle.HintItem.ValueNullable?.Name.ToString() ?? "";
        description = hairstyle.HintItem.ValueNullable?.Description.ToString() ?? "";
        nameLowercase = (hairstyle.HintItem.ValueNullable?.Name.ToString() ?? "").ToLower();
        descriptionLowercase = (hairstyle.HintItem.ValueNullable?.Description.ToString() ?? "").ToLower();
        howTo = tableRow.HowTo;
        howToLowercase = tableRow.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsCharaMakeCustomizeUnlocked(hairstyle);
    }

    public uint Id() => hairstyle.FeatureID;
    public UnlockableType Type() => UnlockableType.Hairstyle;
    public uint Icon() => hairstyle.Icon;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => hairstyle.IsValidEntry();
}
