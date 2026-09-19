using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed record HuntingLogType(int Id, uint Offset) {
    public static readonly HuntingLogType Gla = new(0, 1);
    public static readonly HuntingLogType Pgl = new(1, 2);
    public static readonly HuntingLogType Mrd = new(2, 3);
    public static readonly HuntingLogType Lnc = new(3, 4);
    public static readonly HuntingLogType Arc = new(4, 5);
    public static readonly HuntingLogType Cnj = new(5, 6);
    public static readonly HuntingLogType Thm = new(6, 7);
    public static readonly HuntingLogType Acn = new(7, 26);
    public static readonly HuntingLogType Maelstrom = new(8, 100);
    public static readonly HuntingLogType TwinAdder = new(9, 200);
    public static readonly HuntingLogType ImmortalFlames = new(10, 300);
    public static readonly HuntingLogType Rog = new(11, 29);
    public static readonly List<HuntingLogType> All = [Gla, Pgl, Mrd, Lnc, Arc, Cnj, Thm, Acn, Maelstrom, TwinAdder, ImmortalFlames, Rog];

    public static HuntingLogType GetByMonsterNoteRowId(uint id) => All.First(it => id / 10000 == it.Offset);
}

public sealed record UnlockableHuntingLog : IUnlockable {
    private readonly MonsterNote note;
    private readonly uint icon;
    private readonly uint current;
    private readonly byte max;
    private readonly string name;
    private readonly string nameLowercase;
    private readonly string description;
    private readonly string descriptionLowercase;

    public UnlockableHuntingLog(HuntingLogType type, MonsterNote note, MonsterNoteTarget target, byte max) {
        this.note = note;
        this.max = max;
        icon = (uint)note.MonsterNoteTarget.First().Value.Icon;
        name = GetName(note, target, max);
        nameLowercase = name.ToLower();
        description = GetDescription(target);
        descriptionLowercase = description.ToLower();
        current = (uint)GetCurrent(type, note, target, max);
    }

    public uint Id() => note.RowId;
    public UnlockableType Type() => UnlockableType.HuntingLog;
    public uint Icon() => icon;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => null;
    public string? HowToLowercase() => null;
    public uint? Current() => current;
    public uint Maximum() => max;
    public bool Unlocked() => current == max;
    public bool IsValid() => note.IsValidEntry();

    private static int GetCurrent(HuntingLogType type, MonsterNote note, MonsterNoteTarget target, byte max) {
        var level = (note.RowId - 1) % 50 / 10;
        var currentLevel = Plugin.MonsterNoteManager.Valid ? Plugin.MonsterNoteManager.Value.RankData[type.Id].Rank : 0;

        if (level > currentLevel) return 0;
        if (level < currentLevel) return max;

        var targetIndex = note.MonsterNoteTarget.Index().First(it => it.Item.RowId == target.RowId).Index;

        return Plugin.MonsterNoteManager.Value.RankData[type.Id].RankData[((int)note.RowId - 1) % 10][targetIndex];
    }

    private static string GetName(MonsterNote note, MonsterNoteTarget target, byte max) =>
        $"{note.Name.ToString()} - {target.BNpcName.Value.Singular.ToString()} x{max}";

    private static string GetDescription(MonsterNoteTarget target) =>
        target.PlaceNameLocation.Zip(target.PlaceNameZone).Where(it => it.First.IsValid && it.Second.IsValid && it.First.Value.IsValidEntry())
              .Select(it => $"{it.First.Value.Name.ToString()}" +
                            (it.First.RowId != it.Second.RowId && it.Second.Value.IsValidEntry() ? $" - {it.Second.Value.Name.ToString()}" : ""))
              .Aggregate((acc, el) => acc + '\n' + el);
}
