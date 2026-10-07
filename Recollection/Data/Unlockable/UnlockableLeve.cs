using MethodTimer;
using Dalamud.Utility;
using Recollection.Helpers;
using Leve = Lumina.Excel.Sheets.Leve;

namespace Recollection.Data.Unlockable;

public class UnlockableLeve : IUnlockable {
    private readonly Leve leve;
    private readonly string name;
    private readonly string nameLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly uint icon;
    private readonly bool unlocked;

    [Time]
    public UnlockableLeve(Leve leve) {
        this.leve = leve;
        name = leve.Name.ToString();
        howTo = GetHowTo();
        nameLowercase = name.ToLower();
        howToLowercase = howTo?.ToLower();
        icon = (uint)leve.IconIssuer;
        unlocked = Plugin.UnlockState.IsLeveCompleted(leve);
    }

    public uint Id() => leve.RowId;
    public UnlockableType Type() => UnlockableType.Minion;
    public uint Icon() => icon;
    public string Name() => name;
    public string Description() => "";
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => "";
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => leve.IsValidEntry();

    private string GetHowTo() {
        var genre = leve.JournalGenre.Value.Name.ToString();
        var placeName = leve.LevelLevemete.Value.Territory.Value.PlaceName.Value.Name.ToString();
        var location = MapUtil.WorldToMap(new(leve.LevelLevemete.Value.X, leve.LevelLevemete.Value.Z),
                                              leve.LevelLevemete.Value.Map.Value);

        return $"{genre} - {placeName} (X {location.X:.0}, Y {location.Y:.0})";
    }
}
