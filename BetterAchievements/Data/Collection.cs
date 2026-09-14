using System.Collections.Generic;
using BetterAchievements.External.Lalachievements;

namespace BetterAchievements.Data;

public sealed record CollectionCategory {
    public required uint Id { get; init; }
    public required string Name { get; init; }
    public required List<uint> Items { get; init; }
}
