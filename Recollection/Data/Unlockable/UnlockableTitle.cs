using System;
using System.Collections.Generic;
using System.Linq;
using Recollection.External.Lalachievements;
using Dalamud.Game.Player;
using Recollection.Helpers;
using Title = Lumina.Excel.Sheets.Title;

namespace Recollection.Data.Unlockable;

public sealed record UnlockableTitle : IUnlockable {
    private static readonly Lazy<Dictionary<uint, string>> TitleUnlockAchievementNames =
        new(() => ExcelSheets.Achievement.Value.Where(it => it.Title.IsValid)
                             .ToDictionary(it => it.RowId, it => it.Name.ToString()));

    private readonly Title title;

    public UnlockableTitle(Title title) {
        this.title = title;
        name = GetName(title);
        nameLowercase = GetName(title).ToLower();
        howTo = GetHowTo();
        howToLowercase = GetHowTo().ToLower();
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
    public bool IsValid() => title.IsValidEntry();

    private static string GetName(Title title) {
        var feminine = Plugin.PlayerState.IsLoaded && Plugin.PlayerState.Sex == Sex.Female;
        var text = (feminine ? title.Feminine : title.Masculine).ToString();
        return title.IsPrefix ? $"{text}..." : $"...{text}";
    }

    private string GetHowTo() => TitleUnlockAchievementNames.Value.TryGetValue(Id(), out var achievementName)
                                     ? $"Obtain the achievement \"{achievementName}\"."
                                     : "Could not find the required achievement.";
}
