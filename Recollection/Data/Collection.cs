using System.Collections.Generic;

namespace Recollection.Data;

public sealed record CollectionCategory {
    public required uint Id { get; init; }
    public required string Name { get; init; }
    public required List<uint> Items { get; init; }
}
