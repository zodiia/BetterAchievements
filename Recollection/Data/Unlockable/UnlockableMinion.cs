using Lumina.Excel.Sheets;
using Recollection.External.Lalachievements;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed record UnlockableMinion : IUnlockable {
    private readonly Companion minion;
    private readonly string name;
    private readonly string description;
    private readonly string nameLowercase;
    private readonly string descriptionLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    public UnlockableMinion(Companion minion, ITableRow tabke) {
        this.minion = minion;
        name = minion.Singular.ToString();
        description = ExcelSheets.CompanionTransient.Value.GetRow(minion.RowId).Description.ToString();
        nameLowercase = minion.Singular.ToString().ToLower();
        descriptionLowercase = ExcelSheets.CompanionTransient.Value.GetRow(minion.RowId).DescriptionEnhanced.ToString().ToLower();
        howTo = tabke.HowTo;
        howToLowercase = tabke.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsCompanionUnlocked(minion);
    }

    public uint Id() => minion.RowId;
    public UnlockableType Type() => UnlockableType.Minion;
    public uint Icon() => minion.Icon;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => minion.IsValidEntry();
}
