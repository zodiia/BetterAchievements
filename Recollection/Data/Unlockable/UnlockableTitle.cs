using MethodTimer;
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

    [Time]
    public UnlockableTitle(Title title) {
        this.title = title;
        Name = GetName(title);
        HowTo = GetHowTo();
        NameLowercase = Name.ToLower();
        HowToLowercase = HowTo.ToLower();
        Unlocked = Plugin.UnlockState.IsTitleUnlocked(title);
    }

    public uint Id => title.RowId;
    public UnlockableType Type => UnlockableType.Title;
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

    public bool IsValid() => title.IsValidEntry();

    private static string GetName(Title title) {
        var feminine = Plugin.PlayerState.IsLoaded && Plugin.PlayerState.Sex == Sex.Female;
        var text = (feminine ? title.Feminine : title.Masculine).ToString();
        return title.IsPrefix ? $"{text}..." : $"...{text}";
    }

    private string GetHowTo() => TitleUnlockAchievementNames.Value.TryGetValue(Id, out var achievementName)
                                     ? $"Obtain the achievement \"{achievementName}\"."
                                     : "Could not find the required achievement.";
}
