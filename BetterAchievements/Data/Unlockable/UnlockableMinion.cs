using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableMinion(Companion Minion) : IUnlockable {
    public uint Id() => Minion.RowId;
    public UnlockableType Type() => UnlockableType.Minion;
    public uint Icon() => Minion.Icon;
    private readonly string name = Minion.Singular.ToString();
    public string Name() => name;
    private readonly string description = Plugin.DataManager.GetExcelSheet<CompanionTransient>().GetRow(Minion.RowId).Description.ToString();
    public string Description() => description;
    private readonly string nameLowercase = Minion.Singular.ToString().ToLower();
    public string NameLowercase() => nameLowercase;
    private readonly string descriptionLowercase = Plugin.DataManager.GetExcelSheet<CompanionTransient>().GetRow(Minion.RowId).DescriptionEnhanced.ToString().ToLower();
    public string DescriptionLowercase() => descriptionLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked = Plugin.UnlockState.IsCompanionUnlocked(Minion);
    public bool Unlocked() => unlocked;
}
