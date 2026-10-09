using MethodTimer;
using System;
using Lumina.Excel.Sheets;
using Recollection.Helpers;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableSurveyRecord : IUnlockable {
    private readonly VVDNotebookContents contents;

    [Time]
    public UnlockableSurveyRecord(VVDNotebookContents contents) {
        this.contents = contents;
        Name = contents.Name.ToString();
        Description = contents.Description.ToString();
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        Unlocked = IsCompleted(contents.RowId);
    }

    public uint Id => contents.RowId;
    public UnlockableType Type => UnlockableType.SurveyRecord;
    public uint Icon => (uint)contents.Icon;

    public string Name { get; }
    public string Description { get; }
    public string? HowTo => null;
    public string NameLowercase { get; }
    public string DescriptionLowercase { get; }
    public string? HowToLowercase => null;

    public uint? Current {
        get => Unlocked ? 1u : 0u;
        set => throw new NotSupportedException("Current is derived from Unlocked.");
    }
    public uint Maximum => 1;
    public bool Unlocked { get; set; }

    public bool IsValid() => contents.IsValidEntry();

    public static bool IsCompleted(uint rowId) =>
        Plugin.NativePlayerState.Valid && Plugin.NativePlayerState.Value.CompletedVVDNotebookContentsBitArray[(int)rowId - 1];
}
