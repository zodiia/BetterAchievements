using MethodTimer;
using System;
using System.Numerics;
using Dalamud.Utility;
using Lumina.Excel.Sheets;

namespace Recollection.Data.Unlockable;

/// <summary>
/// All the links:<br />
/// - TripleTriad.RowId = 2293760 + ID<br />
/// - TripleTriadResident.RowId == TripleTriad.RowId<br />
/// - ENpcBase.RowId = 1000000 + Npc ID<br />
/// - ENpcBase.ENpcData[one of them, random position] == TripleTriad.RowId
/// - ENpcResident.RowId == ENpcBase.RowId
/// - Level.Object == ENpcBase.RowId
/// </summary>
public sealed class UnlockableTripleTriadNpc : IUnlockable {
    private readonly TripleTriad tt;
    private readonly Level level;

    [Time]
    public UnlockableTripleTriadNpc(TripleTriad tt, TripleTriadResident ttResident, ENpcResident eNpcResident, Level level) {
        this.tt = tt;
        this.level = level;
        Name = eNpcResident.Singular.ToString();
        HowTo = GetHowTo();
        NameLowercase = Name.ToLower();
        HowToLowercase = HowTo.ToLower();
        Unlocked = Plugin.UiState.Valid && Plugin.UiState.Value.IsTripleTriadNpcBeaten(ttResident.RowId);
    }

    public uint Id => tt.RowId;
    public UnlockableType Type => UnlockableType.TripleTriadNpc;
    public uint Icon => 0;

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

    public bool IsValid() => true; // TODO

    private string GetHowTo() {
        var location = MapUtil.WorldToMap(new Vector2(level.X, level.Y), level.Map.Value);
        return $"{level.Map.Value.PlaceName.Value.Name.ToString()} (X {location.X:.0}, Y {location.Y:.0})";
    }
}
