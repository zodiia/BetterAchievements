using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableHairstyle(CharaMakeCustomize Hairstyle) : IUnlockable {
    public uint Id() => Hairstyle.FeatureID;
    public UnlockableType Type() => UnlockableType.Hairstyle;
    public uint Icon() => Hairstyle.Icon;
    private readonly string name = Hairstyle.HintItem.ValueNullable?.Name.ToString() ?? "";
    public string Name() => name;
    private readonly string description = Hairstyle.HintItem.ValueNullable?.Description.ToString() ?? "";
    public string Description() => description;
    private readonly string nameLowercase = (Hairstyle.HintItem.ValueNullable?.Name.ToString() ?? "").ToLower();
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase = (Hairstyle.HintItem.ValueNullable?.Description.ToString() ?? "").ToLower();
    public string DescriptionLowercase() => descriptionLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked = Plugin.UnlockState.IsCharaMakeCustomizeUnlocked(Hairstyle);
    public bool Unlocked() => unlocked;
}
