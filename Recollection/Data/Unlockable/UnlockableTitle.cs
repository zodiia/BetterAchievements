using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Player;
using Recollection.Helpers;
using Title = Lumina.Excel.Sheets.Title;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableTitle : IUnlockable {
    private static readonly Lazy<Dictionary<uint, string>> TitleUnlockAchievementNames =
        new(() => ExcelSheets.Achievement.Value.Where(it => it.Title.IsValid)
                             .ToDictionary(it => it.RowId, it => it.Name.ToString()));

    private readonly Title title;
    private readonly string name;
    private readonly string nameLowercase;
    private readonly string? howTo;
    private readonly string? howToLowercase;
    private readonly bool unlocked;

    public UnlockableTitle(Title title) {
        this.title = title;
        name = GetName(title);
        howTo = GetHowTo();
        nameLowercase = name.ToLower();
        howToLowercase = howTo.ToLower();
        unlocked = Plugin.UnlockState.IsTitleUnlocked(title);
    }

    public uint Id() => title.RowId;
    public UnlockableType Type() => UnlockableType.Title;
    public uint Icon() => 0;
    public string Name() => name;
    public string Description() => "";
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => "";
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
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
