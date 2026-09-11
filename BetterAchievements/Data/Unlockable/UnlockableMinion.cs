using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableMinion : IUnlockable {
    private readonly Companion minion;

    public UnlockableMinion(Companion minion, ITable tabke) {
        this.minion = minion;
        name = minion.Singular.ToString();
        description = Plugin.DataManager.GetExcelSheet<CompanionTransient>().GetRow(minion.RowId).Description.ToString();
        nameLowercase = minion.Singular.ToString().ToLower();
        descriptionLowercase = Plugin.DataManager.GetExcelSheet<CompanionTransient>().GetRow(minion.RowId).DescriptionEnhanced.ToString().ToLower();
        howTo = tabke.HowTo;
        howToLowercase = tabke.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsCompanionUnlocked(minion);
    }

    public uint Id() => minion.RowId;
    public UnlockableType Type() => UnlockableType.Minion;
    public uint Icon() => minion.Icon;
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
