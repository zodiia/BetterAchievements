using System.Linq;
using BetterAchievements.External.Lalachievements;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

/// <summary>
/// All the links:<br />
/// - TripleTriad.RowId = 2293760 + ID<br />
/// - TripleTriadResident.RowId == TripleTriad.RowId<br />
/// - ENpcBase.RowId = 1000000 + Npc ID<br />
/// - ENpcBase.ENpcData[one of them, random position] == TripleTriad.RowId
/// - ENpcResident.RowId == ENpcBase.RowId
/// - Level.Object == ENpcBase.RowId
/// </summary>
public sealed record UnlockableTripleTriadNpc : IUnlockable {
    private readonly TripleTriad tt;
    private readonly Level level;
    private readonly string name;
    private readonly string howTo;
    private readonly string nameLowercase;
    private readonly string howToLowercase;
    private readonly bool unlocked;

    public UnlockableTripleTriadNpc(TripleTriad tt, TripleTriadResident ttResident, ENpcResident eNpcResident, Level level) {
        this.tt = tt;
        this.level = level;
        name = eNpcResident.Singular.ToString();
        howTo = GetHowTo();
        nameLowercase = eNpcResident.Singular.ToString().ToLower();
        howToLowercase = GetHowTo().ToLower();
        unlocked = Plugin.UiState.Valid && Plugin.UiState.Value.IsTripleTriadNpcBeaten(ttResident.RowId);
    }

    public uint Id() => tt.RowId;
    public UnlockableType Type() => UnlockableType.Emote;
    public uint Icon() => 0; // TODO: pretty sure there's an icon somewhere
    public string Name() => name;
    public string Description() => "";
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => "";
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => true; // TODO

    private string GetHowTo() => $"{level.Map.Value.PlaceName.Value.Name.ToString()} (X {level.X:.1}, Y {level.Y:.1}, Z {level.Z:.1})";
}
