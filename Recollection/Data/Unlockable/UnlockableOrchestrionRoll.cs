using MethodTimer;
using System;
using Lumina.Excel.Sheets;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableOrchestrionRoll : IUnlockable {
    private const uint StaticIcon = 25945;
    private readonly Orchestrion orchestrion;

    [Time]
    public UnlockableOrchestrionRoll(Orchestrion orchestrion) {
        this.orchestrion = orchestrion;
        Name = orchestrion.Name.ToString();
        HowTo = orchestrion.Description.ToString();
        NameLowercase = Name.ToLower();
        HowToLowercase = HowTo.ToLower();
        Unlocked = Plugin.UnlockState.IsOrchestrionUnlocked(orchestrion);
    }

    public uint Id => orchestrion.RowId;
    public UnlockableType Type => UnlockableType.OrchestrionRoll;
    public uint Icon => StaticIcon;

    public string Name { get; }
    public string? Description => null;
    public string HowTo { get; }
    public string NameLowercase { get; }
    public string? DescriptionLowercase => null;
    public string HowToLowercase { get; }

    public uint? Current {
        get => Unlocked ? 1u : 0u;
        set => throw new NotSupportedException("Current is derived from Unlocked.");
    }
    public uint Maximum => 1;
    public bool Unlocked { get; set; }

    public bool IsValid() => orchestrion.IsValidEntry();
}
