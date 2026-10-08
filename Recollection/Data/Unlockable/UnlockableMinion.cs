using MethodTimer;
using System;
using Lumina.Excel.Sheets;
using Recollection.External.Lalachievements;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableMinion : IUnlockable {
    private readonly Companion minion;

    [Time]
    public UnlockableMinion(Companion minion, ITableRow table) {
        this.minion = minion;
        Name = minion.Singular.ToString();
        Description = ExcelSheets.CompanionTransient.Value.GetRow(minion.RowId).Description.ToString();
        HowTo = table.HowTo;
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        HowToLowercase = HowTo?.ToLower();
        Unlocked = Plugin.UnlockState.IsCompanionUnlocked(minion);
    }

    public uint Id => minion.RowId;
    public UnlockableType Type => UnlockableType.Minion;
    public uint Icon => minion.Icon;

    public string Name { get; }
    public string? Description { get; }
    public string? HowTo { get; }
    public string NameLowercase { get; }
    public string? DescriptionLowercase { get; }
    public string? HowToLowercase { get; }

    public uint? Current {
        get => Unlocked ? 1u : 0u;
        set => throw new NotSupportedException("Current is derived from Unlocked.");
    }
    public uint Maximum => 1;
    public bool Unlocked { get; set; }

    public bool IsValid() => minion.IsValidEntry();
}
