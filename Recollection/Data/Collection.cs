using System.Collections.Generic;
using Recollection.External.Lalachievements;

namespace Recollection.Data;

public sealed record CollectionCategory {
    public required uint Id { get; init; }
    public required string Name { get; init; }
    public required List<uint> Items { get; init; }
}
