using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Serilog;

namespace BetterAchievements.Lalachievements;

public class LalachievementsService
{
    public ConcurrentDictionary<uint, uint> AchievementRarity = new();

    public LalachievementsService()
    {
        GetAchievementRarity();
    }

    public async void GetAchievementRarity()
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetFromJsonAsync<RarityResponse>("https://lalachievements.com/api/rarity/achievements/global",
                                                                         new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (response is not null)
            {
                response.Rarity.Sort((a, b) => a.Percentile.CompareTo(b.Percentile));
                var idx = 0u;
                foreach (var it in response.Rarity)
                {
                    AchievementRarity[idx] = it.Id;
                    idx++;
                }
            }
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error requesting Lalachievements for achievement rarity.");
        }
    }
}
