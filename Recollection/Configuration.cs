using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Recollection.Data;

namespace Recollection;

[Serializable]
public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 11;

    // UI settings
    public float ProgressBarHeight = 1.5f;
    public float SidebarProgressBarHeight = 0.2f;
    public float UiDensity = 1.0f;
    public TieredAchievementDisplay TieredAchievementDisplay = TieredAchievementDisplay.All;
    public bool NeverHideProgressBars = false;
    public bool DisableRomanNumerals = false;

    // Filters and sorting options
    public UnlockStatusFilter UnlockStatusFilter = UnlockStatusFilter.All;
    public ContainsRewardsFilter ContainsRewardsFilter = ContainsRewardsFilter.All;
    public RankedFilter RankedFilter = RankedFilter.All;
    public AreaFilter AreaFilter = AreaFilter.All;
    public ProgressionFilter ProgressionFilter = ProgressionFilter.All;
    public SortBy SortBy = SortBy.Default;

    // Other settings
    public bool DisplayIds = false;

    // Achievement roulette settings, not in settings view
    public HashSet<string> RouletteDisabledCategories = [];
    public int RouletteCardCount = 2;
    public int RouletteMinPoints = 0;
    public int RouletteMaxPoints = 50;
    public bool RouletteNextTierOnly = false;
    public bool RouletteOnlyWithCounter = false;

    // TODO: Remove
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
