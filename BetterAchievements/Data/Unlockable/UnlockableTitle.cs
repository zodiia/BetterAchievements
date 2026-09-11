using Dalamud.Game.Player;
using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableTitle : IUnlockable {
    private readonly Title title;

    public UnlockableTitle(Title title, ITable table) {
        this.title = title;
        name = DisplayName(title);
        nameLowercase = DisplayName(title).ToLower();
        howTo = table.HowTo;
        howToLowercase = table.HowTo?.ToLower();
        unlocked = Plugin.UnlockState.IsTitleUnlocked(title);
    }

    public uint Id() => title.RowId;
    public UnlockableType Type() => UnlockableType.Title;
    public uint Icon() => 0;
    private readonly string name;
    public string Name() => name;
    public string Description() => "";
    private readonly string nameLowercase;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => "";
    private readonly string? howTo;
    public string? HowTo() => howTo;
    private readonly string? howToLowercase;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked;
    public bool Unlocked() => unlocked;

    private static string DisplayName(Title title) {
        var feminine = Plugin.PlayerState.IsLoaded && Plugin.PlayerState.Sex == Sex.Female;
        var text = (feminine ? title.Feminine : title.Masculine).ToString();
        return title.IsPrefix ? $"{text}..." : $"...{text}";
    }
}
