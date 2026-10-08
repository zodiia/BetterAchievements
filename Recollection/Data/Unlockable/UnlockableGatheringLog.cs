using MethodTimer;
using System;
using System.Numerics;
using Recollection.Helpers;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace Recollection.Data.Unlockable;

public sealed class UnlockableGatheringLog : IUnlockable {
    private readonly GatheringItem item;
    private readonly GatheringPoint point;
    private readonly ExportedGatheringPoint exported;

    [Time]
    public UnlockableGatheringLog(GatheringItem item, GatheringPoint point, ExportedGatheringPoint exported) {
        this.item = item;
        this.point = point;
        this.exported = exported;
        Icon = item.Item.GetValueOrDefault<Item>()?.Icon ?? 0;
        Name = item.Item.GetValueOrDefault<Item>()?.Name.ToString() ?? "Unknown";
        Description = item.Item.GetValueOrDefault<Item>()?.Description.ToString() ?? "Unknown";
        HowTo = GetHowTo();
        NameLowercase = Name.ToLower();
        DescriptionLowercase = Description.ToLower();
        HowToLowercase = HowTo.ToLower();
        Unlocked = Plugin.QuestManager.Valid && QuestManager.IsGatheringItemGathered((ushort)item.RowId);
    }

    public uint Id => item.RowId;
    public UnlockableType Type => UnlockableType.GatheringLog;
    public uint Icon { get; }

    public string Name { get; }
    public string? Description { get; }
    public string HowTo { get; }
    public string NameLowercase { get; }
    public string? DescriptionLowercase { get; }
    public string HowToLowercase { get; }

    public uint? Current {
        get => Unlocked ? 1u : 0u;
        set => throw new NotSupportedException("Current is derived from Unlocked.");
    }
    public uint Maximum => 1;
    public bool Unlocked { get; set; }

    public bool IsValid() => item.IsValidEntry();

    private string GetHowTo() {
        var location = MapUtil.WorldToMap(new Vector2(exported.X, exported.Y), point.TerritoryType.Value.Map.Value);
        return $"{point.PlaceName.Value.Name.ToString()} ({point.TerritoryType.Value.PlaceName.Value.Name.ToString()}, X {location.X:.0}, Z {location.Y:.0})";
    }
}
