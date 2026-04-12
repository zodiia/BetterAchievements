using System.Collections.Generic;

namespace BetterAchievements.External.Lalachievements;

public record RarityResponse(string CharCount, List<RarityEntry> Rarity);

public record RarityEntry(uint Id, string Count, string PointsCount, double Percentile, int Points);
