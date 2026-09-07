using System.Collections.Generic;
using BetterAchievements.External.Lalachievements;

namespace BetterAchievements.Data;

public sealed record CollectionCategory {
    public required uint Id { get; init; }
    public required string Name { get; init; }
    public required List<CollectionItem> Items { get; init; }
}

public sealed record CollectionItem {
    public required uint Id { get; init; }
    public required ITable Table { get; init; }
    public required SourceType? SourceType { get; init; }
    public required string HowTo { get; init; }
}
