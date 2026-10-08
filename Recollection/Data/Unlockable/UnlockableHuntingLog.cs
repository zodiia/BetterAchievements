using MethodTimer;
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

public sealed class UnlockableHuntingLog : IUnlockable {
    private readonly MonsterNote note;

    [Time]
    public UnlockableHuntingLog(HuntingLogType type, MonsterNote note, MonsterNoteTarget target, byte max) {
        this.note = note;
        Icon = (uint)note.MonsterNoteTarget.First().Value.Icon;
        Name = GetName(note, target, max);
        Description = GetDescription(target);
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        Current = (uint)GetCurrent(type, note, target, max);
        Maximum = max;
        Unlocked = Current == Maximum;
    }

    public uint Id => note.RowId;
    public UnlockableType Type => UnlockableType.HuntingLog;
    public uint Icon { get; }

    public string Name { get; }
    public string? Description { get; }
    public string? HowTo => null;
    public string NameLowercase { get; }
    public string? DescriptionLowercase { get; }
    public string? HowToLowercase => null;

    public uint? Current { get; set; }
    public uint Maximum { get; }
    public bool Unlocked { get; set; }

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
