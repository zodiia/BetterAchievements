using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableHairstyle : IUnlockable {
    private readonly CharaMakeCustomize hairstyle;

    public UnlockableHairstyle(CharaMakeCustomize hairstyle, ITable table) {
        this.hairstyle = hairstyle;
        name = hairstyle.HintItem.ValueNullable?.Name.ToString() ?? "";
        description = hairstyle.HintItem.ValueNullable?.Description.ToString() ?? "";
        nameLowercase = (hairstyle.HintItem.ValueNullable?.Name.ToString() ?? "").ToLower();
        descriptionLowercase = (hairstyle.HintItem.ValueNullable?.Description.ToString() ?? "").ToLower();
        howTo = table.HowTo;
        howToLowercase = table.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsCharaMakeCustomizeUnlocked(hairstyle);
    }

    public uint Id() => hairstyle.FeatureID;
    public UnlockableType Type() => UnlockableType.Hairstyle;
    public uint Icon() => hairstyle.Icon;
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
