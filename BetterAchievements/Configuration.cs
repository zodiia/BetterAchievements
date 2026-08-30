using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using BetterAchievements.Data;

namespace BetterAchievements;

[Serializable]
public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 4;

    public UnlockStatusFilter UnlockStatusFilter { get; set; } = UnlockStatusFilter.All;
    public ContainsRewardsFilter ContainsRewardsFilter { get; set; } = ContainsRewardsFilter.All;
    public RankedFilter RankedFilter { get; set; } = RankedFilter.All;
    public AreaFilter AreaFilter { get; set; } = AreaFilter.All;
    public SortBy SortBy { get; set; } = SortBy.Default;
    public bool DisplayIds { get; set; } = false;
    public bool NeverHideProgressBars { get; set; } = false;
    public bool DebugMode { get; set; } = false;

    // Not shown in the config UI
    public List<uint> PinnedAchievements { get; set; } = new();

    // The below exists just to make saving less cumbersome
    public void Save() {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
