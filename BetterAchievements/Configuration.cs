using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Numerics;
using BetterAchievements.Data;
using Newtonsoft.Json;

namespace BetterAchievements;

[Serializable]
public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 5;

    // UI settings
    public float ProgressBarHeight = 1.5f;
    public float SidebarProgressBarHeight = 0.2f;
    public bool NeverHideProgressBars { get; set; } = false;

    // Filters and sorting options
    public UnlockStatusFilter UnlockStatusFilter { get; set; } = UnlockStatusFilter.All;
    public ContainsRewardsFilter ContainsRewardsFilter { get; set; } = ContainsRewardsFilter.All;
    public RankedFilter RankedFilter { get; set; } = RankedFilter.All;
    public AreaFilter AreaFilter { get; set; } = AreaFilter.All;
    public SortBy SortBy { get; set; } = SortBy.Default;

    // Other settings
    public bool DisplayIds { get; set; } = false;
    public bool DebugMode { get; set; } = false;

    // Not shown in the config UI
    public List<uint> PinnedAchievements { get; set; } = new();

    // The below exists just to make saving less cumbersome
    public void Save() {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public Configuration Clone() {
        var settings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
        return JsonConvert.DeserializeObject<Configuration>(JsonConvert.SerializeObject(this, settings), settings)!;
    }
}
