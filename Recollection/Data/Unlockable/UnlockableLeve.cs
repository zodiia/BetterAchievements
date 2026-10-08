using MethodTimer;
using System;
using Dalamud.Utility;
using Recollection.Helpers;
using Leve = Lumina.Excel.Sheets.Leve;

namespace Recollection.Data.Unlockable;

public class UnlockableLeve : IUnlockable {
    private readonly Leve leve;

    [Time]
    public UnlockableLeve(Leve leve) {
        this.leve = leve;
        Icon = (uint)leve.IconIssuer;
        Name = leve.Name.ToString();
        HowTo = GetHowTo();
        NameLowercase = Name.ToLower();
        HowToLowercase = HowTo.ToLower();
        Unlocked = Plugin.UnlockState.IsLeveCompleted(leve);
    }

    public uint Id => leve.RowId;
    public UnlockableType Type => UnlockableType.Leve;
    public uint Icon { get; }

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

    public bool IsValid() => leve.IsValidEntry();

    private string GetHowTo() {
        var genre = leve.JournalGenre.Value.Name.ToString();
        var placeName = leve.LevelLevemete.Value.Territory.Value.PlaceName.Value.Name.ToString();
        var location = MapUtil.WorldToMap(new(leve.LevelLevemete.Value.X, leve.LevelLevemete.Value.Z),
                                              leve.LevelLevemete.Value.Map.Value);

        return $"{genre} - {placeName} (X {location.X:.0}, Y {location.Y:.0})";
    }
}
