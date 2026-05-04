using System.Collections.Generic;

namespace BetterAchievements.External.Lalachievements;

public record RarityResponse(string CharCount, List<RarityEntry> Rarity);

public record RarityEntry(uint Id, string Count = "0", string PointsCount = "0", double Percentile = 0.0f, int Points = 0);
