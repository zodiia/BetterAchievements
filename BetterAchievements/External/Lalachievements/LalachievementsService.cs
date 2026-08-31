using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Dalamud.Plugin.Services;

namespace BetterAchievements.External.Lalachievements;

public class LalachievementsService {
    public static readonly IPluginLog Log = Plugin.GetLogger<LalachievementsService>();

    public readonly ConcurrentDictionary<uint, uint> AchievementRarity = new();

    public LalachievementsService() {
        GetAchievementRarity();
    }

    // this is dummy data
    public uint? GetWorldRank() => 128;
    public uint? GetDataCenterRank() => 542;
    public uint? GetGlobalRank() => 9134;

    public async void GetAchievementRarity() {
        try {
            var client = new HttpClient();
            var response = await client.GetFromJsonAsync<RarityResponse>("https://lalachievements.com/api/rarity/achievements/global",
                                                                         new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (response is not null) {
                var idx = 0u;
                var rarities = response.Rarity.Where(it => it.Points is > 0).ToList();

                rarities.Sort((a, b) => (a.Percentile ?? 0.0).CompareTo(b.Percentile ?? 0.0));
                foreach (var it in rarities) {
                    AchievementRarity[it.Id] = idx;
                    idx++;
                }
            }
        } catch (Exception exception) {
            Log.Error(exception, "Error requesting Lalachievements for achievement rarity");
        }
    }
}
