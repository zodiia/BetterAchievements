using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Dalamud.Plugin.Services;

namespace BetterAchievements.External.Lalachievements;

public class LalachievementsService {
    private const string GameAllUrl = "https://lalachievements.com/api/game/en/all";
    private const string AchievementRarityUrl = "https://lalachievements.com/api/rarity/achievements/global";
    private const string GameAllCacheFileName = "lalachievements-all.json";
    private static readonly TimeSpan GameAllCacheDuration = TimeSpan.FromHours(24);

    private static readonly JsonSerializerOptions GameAllJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static readonly IPluginLog Log = Plugin.GetLogger<LalachievementsService>();

    public readonly ConcurrentDictionary<uint, uint> AchievementRarity = new();

    public GameAllResponse? GameAll { get; private set; }

    public LalachievementsService() {
        GetAchievementRarity();
        GetGameAll();
    }

    public List<T> GetTable<T>() where T : ITable => GameAll?.GetTable<T>() ?? [];

    // this is dummy data
    public uint? GetWorldRank() => 128;
    public uint? GetDataCenterRank() => 542;
    public uint? GetGlobalRank() => 9134;

    public async void GetAchievementRarity() {
        try {
            var client = new HttpClient();
            var response = await client.GetFromJsonAsync<RarityResponse>(AchievementRarityUrl,
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

    private async void GetGameAll() {
        try {
            var gameVersion = Plugin.DataManager.GameData.Repositories.First().Value.Version;
            var cache = ReadGameAllCache();

            if (cache is not null && cache.GameVersion == gameVersion && DateTimeOffset.UtcNow - cache.CachedAt < GameAllCacheDuration) {
                GameAll = cache.Response;
                Log.Info("Loaded the Lalachievements game data from cache");
                return;
            }

            var client = new HttpClient();
            var response = await client.GetFromJsonAsync<GameAllResponse>(GameAllUrl, GameAllJsonOptions);

            if (response is null) {
                return;
            }

            GameAll = response;
            WriteGameAllCache(new GameAllCache { GameVersion = gameVersion, CachedAt = DateTimeOffset.UtcNow, Response = response });
        } catch (Exception exception) {
            Log.Error(exception, "Error requesting Lalachievements for game data");
        }
    }

    private static string GameAllCachePath => Path.Combine(Plugin.PluginInterface.ConfigDirectory.FullName, GameAllCacheFileName);

    private static GameAllCache? ReadGameAllCache() {
        if (!File.Exists(GameAllCachePath)) {
            return null;
        }

        try {
            using var stream = File.OpenRead(GameAllCachePath);
            return JsonSerializer.Deserialize<GameAllCache>(stream, GameAllJsonOptions);
        } catch (Exception exception) {
            Log.Warning(exception, "Could not read the Lalachievements game data cache");
            return null;
        }
    }

    private static void WriteGameAllCache(GameAllCache cache) {
        try {
            using var stream = File.Create(GameAllCachePath);
            JsonSerializer.Serialize(stream, cache, GameAllJsonOptions);
        } catch (Exception exception) {
            Log.Warning(exception, "Could not write the Lalachievements game data cache");
        }
    }
}
