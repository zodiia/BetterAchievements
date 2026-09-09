using Dalamud.Game.Player;
using Lumina.Excel.Sheets;
using ITable = BetterAchievements.External.Lalachievements.ITable;

namespace BetterAchievements.Data.Unlockable;

public sealed record UnlockableTitle(Title Title, ITable Table) : IUnlockable {
    public uint Id() => Title.RowId;
    public UnlockableType Type() => UnlockableType.Title;
    public uint Icon() => 0;
    private readonly string name = DisplayName(Title);
    public string Name() => name;
    public string Description() => "";
    private readonly string nameLowercase = DisplayName(Title).ToLower();
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => "";
    private readonly string howTo = Table.PlainHowTo();
    public string HowTo() => howTo;
    private readonly string howToLowercase = Table.PlainHowTo().ToLower();
    public string HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    private readonly bool unlocked = Plugin.UnlockState.IsTitleUnlocked(Title);
    public bool Unlocked() => unlocked;

    private static string DisplayName(Title title) {
        var feminine = Plugin.PlayerState.IsLoaded && Plugin.PlayerState.Sex == Sex.Female;
        var text = (feminine ? title.Feminine : title.Masculine).ToString();
        return title.IsPrefix ? $"{text}..." : $"...{text}";
    }
}
