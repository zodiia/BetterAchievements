using Lumina.Excel.Sheets;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed record UnlockableOrchestrionRoll : IUnlockable {
    private const uint StaticIcon = 25945;
    private readonly Orchestrion orchestrion;
    private readonly string name;
    private readonly string nameLowercase;
    private readonly string howTo;
    private readonly string howToLowercase;
    private readonly bool unlocked;

    public UnlockableOrchestrionRoll(Orchestrion orchestrion) {
        this.orchestrion = orchestrion;
        name = orchestrion.Name.ToString();
        nameLowercase = orchestrion.Name.ToString().ToLower();
        howTo = orchestrion.Description.ToString();
        howToLowercase = orchestrion.Description.ToString().ToLower();
        unlocked = Plugin.UnlockState.IsOrchestrionUnlocked(orchestrion);
    }

    public uint Id() => orchestrion.RowId;
    public UnlockableType Type() => UnlockableType.OrchestrionRoll;
    public uint Icon() => StaticIcon;
    public string Name() => name;
    public string Description() => "";
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => "";
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => orchestrion.IsValidEntry();
}
