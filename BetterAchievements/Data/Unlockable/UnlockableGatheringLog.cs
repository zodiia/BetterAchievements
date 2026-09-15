using System.Numerics;
using BetterAchievements.Helpers;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace BetterAchievements.Data.Unlockable;

public class UnlockableGatheringLog : IUnlockable {
    private readonly GatheringItem item;
    private readonly GatheringPoint point;
    private readonly ExportedGatheringPoint exported;
    private readonly string name;
    private readonly string nameLowercase;
    private readonly string description;
    private readonly string descriptionLowercase;
    private readonly string howTo;
    private readonly string howToLowercase;
    private readonly uint icon;
    private readonly bool unlocked;

    public UnlockableGatheringLog(GatheringItem item, GatheringPoint point, ExportedGatheringPoint exported) {
        this.item = item;
        this.point = point;
        this.exported = exported;
        name = item.Item.GetValueOrDefault<Item>()?.Name.ToString() ?? "Unknown";
        description = item.Item.GetValueOrDefault<Item>()?.Description.ToString() ?? "Unknown";
        nameLowercase = item.Item.GetValueOrDefault<Item>()?.Name.ToString().ToLower() ?? "unknown";
        descriptionLowercase = item.Item.GetValueOrDefault<Item>()?.Description.ToString().ToLower() ?? "unknown";
        howTo = GetHowTo();
        howToLowercase = howTo.ToLower();
        icon = item.Item.GetValueOrDefault<Item>()?.Icon ?? 0;
        unlocked = Plugin.QuestManager.Valid && QuestManager.IsGatheringItemGathered((ushort)item.RowId);
    }

    public uint Id() => item.RowId;
    public UnlockableType Type() => UnlockableType.GatheringLog;
    public uint Icon() => icon;
    public string Name() => name;
    public string Description() => description;
    public string NameLowercase() => nameLowercase;
    public string DescriptionLowercase() => descriptionLowercase;
    public string? HowTo() => howTo;
    public string? HowToLowercase() => howToLowercase;
    public uint? Current() => Unlocked() ? 1u : 0u;
    public uint Maximum() => 1;
    public bool Unlocked() => unlocked;
    public bool IsValid() => item.IsValidEntry();

    private string GetHowTo() {
        var location = MapUtil.WorldToMap(new Vector2(exported.X, exported.Y), point.TerritoryType.Value.Map.Value);
        return $"{point.PlaceName.Value.Name.ToString()} ({point.TerritoryType.Value.PlaceName.Value.Name.ToString()}, X {location.X:.0}, Z {location.Y:.0})";
    }
}
