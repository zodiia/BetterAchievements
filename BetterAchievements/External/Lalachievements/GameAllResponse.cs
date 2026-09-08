using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BetterAchievements.External.Lalachievements;

public interface ITable {
    uint Id { get; init; }
    long UpdatedAt { get; init; }
    bool? Deleted { get; init; }
}

public sealed record GameAllCache {
    public required string GameVersion { get; init; }
    public required DateTimeOffset CachedAt { get; init; }
    public required GameAllResponse Response { get; init; }
}

public sealed record GameAllResponse {
    private readonly ConcurrentDictionary<Type, Dictionary<uint, ITable>> rowsById = new();

    public long GenerateTime { get; init; }
    public long DeployTime { get; init; }
    public required GameAllTables Tables { get; init; }

    public List<T> GetTable<T>() where T : ITable => Tables.GetTable<T>();

    public T? GetRow<T>(uint id) where T : class, ITable {
        var rows = rowsById.GetOrAdd(typeof(T), _ => GetTable<T>().ToDictionary(it => it.Id, ITable (it) => it));

        return rows.GetValueOrDefault(id) as T;
    }

    public SourceType GetSourceType(uint? sourceTypeId) =>
        (sourceTypeId.HasValue ? GetRow<SourceType>(sourceTypeId.Value) : null)
        ?? GetTable<SourceType>().First(it => it.Name == "Unknown");
}

public sealed record GameAllTables {
    public required List<Barding> Bardings { get; init; }
    public required List<BozjaFate> BozjaFates { get; init; }
    public required List<BozjaNote> BozjaNotes { get; init; }
    public required List<Emote> Emotes { get; init; }
    public required List<Fashion> Fashions { get; init; }
    public required List<Fish> Fish { get; init; }
    public required List<Spectacle> Spectacles { get; init; }
    public required List<Hair> Hairs { get; init; }
    public required List<Item> Items { get; init; }
    public required List<Leve> Leves { get; init; }
    public required List<Title> Titles { get; init; }
    public required List<Mount> Mounts { get; init; }
    public required List<Minion> Minions { get; init; }
    public required List<Npc> Npcs { get; init; }
    public required List<Orchestrion> Orchestrions { get; init; }
    public required List<PlaceName> PlaceNames { get; init; }
    public required List<PortraitCondition> PortraitConditions { get; init; }
    public required List<Portrait> Portraits { get; init; }
    public required List<Raid> Raids { get; init; }
    public required List<SourceType> SourceTypes { get; init; }

    [JsonPropertyName("ttcardTypes")]
    public required List<TripleTriadCardType> TripleTriadCardTypes { get; init; }

    [JsonPropertyName("ttcards")]
    public required List<TripleTriadCard> TripleTriadCards { get; init; }

    [JsonPropertyName("ttrules")]
    public required List<TripleTriadRule> TripleTriadRules { get; init; }

    [JsonPropertyName("ttnpcs")]
    public required List<TripleTriadNpc> TripleTriadNpcs { get; init; }

    public List<T> GetTable<T>() where T : ITable {
        object table = typeof(T).Name switch {
            nameof(Barding) => Bardings,
            nameof(BozjaFate) => BozjaFates,
            nameof(BozjaNote) => BozjaNotes,
            nameof(Emote) => Emotes,
            nameof(Fashion) => Fashions,
            nameof(Fish) => Fish,
            nameof(Spectacle) => Spectacles,
            nameof(Hair) => Hairs,
            nameof(Item) => Items,
            nameof(Leve) => Leves,
            nameof(Title) => Titles,
            nameof(Mount) => Mounts,
            nameof(Minion) => Minions,
            nameof(Npc) => Npcs,
            nameof(Orchestrion) => Orchestrions,
            nameof(PlaceName) => PlaceNames,
            nameof(PortraitCondition) => PortraitConditions,
            nameof(Portrait) => Portraits,
            nameof(Raid) => Raids,
            nameof(SourceType) => SourceTypes,
            nameof(TripleTriadCardType) => TripleTriadCardTypes,
            nameof(TripleTriadCard) => TripleTriadCards,
            nameof(TripleTriadRule) => TripleTriadRules,
            nameof(TripleTriadNpc) => TripleTriadNpcs,
            _ => throw new ArgumentException($"No Lalachievements table exists for type {typeof(T).Name}"),
        };

        return (List<T>) table;
    }
}

public sealed record SourceType : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public string Name { get; init; } = "";
}

public sealed record Item : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public string Name { get; init; } = "";
}

public sealed record PlaceName : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public string Name { get; init; } = "";
}

public sealed record Barding : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint Sort { get; init; }
    public uint? SourceTypeId { get; init; }
    public bool? Obtainable { get; init; }
    public string Name { get; init; } = "";
    public string HowTo { get; init; } = "";

    public SourceType GetSourceType(GameAllResponse response) => response.GetSourceType(SourceTypeId);
}

public sealed record Hair : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint? SourceTypeId { get; init; }
    public bool? Obtainable { get; init; }
    public string Name { get; init; } = "";
    public string HowTo { get; init; } = "";

    public SourceType GetSourceType(GameAllResponse response) => response.GetSourceType(SourceTypeId);
}

public sealed record Fashion : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint Sort { get; init; }
    public uint? SourceTypeId { get; init; }
    public bool? Obtainable { get; init; }
    public string Name { get; init; } = "";
    public string HowTo { get; init; } = "";

    public SourceType GetSourceType(GameAllResponse response) => response.GetSourceType(SourceTypeId);
}

public sealed record Spectacle : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint Sort { get; init; }
    public uint? SourceTypeId { get; init; }
    public bool? Obtainable { get; init; }
    public string Name { get; init; } = "";
    public string HowTo { get; init; } = "";

    public SourceType GetSourceType(GameAllResponse response) => response.GetSourceType(SourceTypeId);
}

public sealed record Emote : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint EmoteCategoryId { get; init; }
    public uint Sort { get; init; }
    public uint? SourceTypeId { get; init; }
    public bool? Obtainable { get; init; }
    public string Name { get; init; } = "";
    public string HowTo { get; init; } = "";

    public SourceType GetSourceType(GameAllResponse response) => response.GetSourceType(SourceTypeId);
}

public sealed record Mount : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public bool? Obtainable { get; init; }
    public uint? SourceTypeId { get; init; }
    public uint Sort { get; init; }
    public string Patch { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string DescriptionEnhanced { get; init; } = "";
    public string Tooltip { get; init; } = "";
    public string HowTo { get; init; } = "";

    public SourceType GetSourceType(GameAllResponse response) => response.GetSourceType(SourceTypeId);
}

public sealed record Minion : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public bool? Obtainable { get; init; }
    public uint? SourceTypeId { get; init; }
    public uint Sort { get; init; }
    public string Patch { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string DescriptionEnhanced { get; init; } = "";
    public string Tooltip { get; init; } = "";
    public string HowTo { get; init; } = "";

    public SourceType GetSourceType(GameAllResponse response) => response.GetSourceType(SourceTypeId);
}

public sealed record Orchestrion : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint Sort { get; init; }
    public uint OrchestrionCategoryId { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
}

public sealed record Title : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public bool IsPrefix { get; init; }
    public uint Sort { get; init; }
    public string Masculine { get; init; } = "";
    public string Feminine { get; init; } = "";
}

public sealed record Fish : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public bool IsInLog { get; init; }
    public uint ItemId { get; init; }
    public bool Spearfish { get; init; }
    public uint? Sort { get; init; }
    public string Name { get; init; } = "";
}

public sealed record Raid : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public DateTimeOffset Released { get; init; }
    public uint Expansion { get; init; }
    public string Name { get; init; } = "";
}

public sealed record Npc : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public string DisplayX { get; init; } = "";
    public string DisplayY { get; init; } = "";
    public uint MapId { get; init; }
    public uint SizeFactor { get; init; }
    public uint PlaceNameId { get; init; }
    public uint PlaceNameZoneId { get; init; }
    public uint PlaceNameRegionId { get; init; }
    public string Name { get; init; } = "";

    public PlaceName? GetPlaceName(GameAllResponse response) => response.GetRow<PlaceName>(PlaceNameId);
    public PlaceName? GetPlaceNameZone(GameAllResponse response) => response.GetRow<PlaceName>(PlaceNameZoneId);
    public PlaceName? GetPlaceNameRegion(GameAllResponse response) => response.GetRow<PlaceName>(PlaceNameRegionId);
}

public sealed record Leve : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint LeveAssignmentTypeId { get; init; }
    public uint ClassJobLevel { get; init; }
    public uint ExpReward { get; init; }
    public uint GilReward { get; init; }
    public string PlateIcon { get; init; } = "";
    public string FrameIcon { get; init; } = "";
    public string IssuerIcon { get; init; } = "";
    public string CityStateIcon { get; init; } = "";
    public bool LargeScale { get; init; }
    public bool IsRemoved { get; init; }
    public bool IsUnlock { get; init; }
    public uint OriginNpcId { get; init; }
    public uint DestinationNpcId { get; init; }
    public uint ItemId { get; init; }
    public uint ItemCount { get; init; }
    public uint ItemRepeats { get; init; }
    public uint JournalSort { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string LeveClient { get; init; } = "";
    public string JournalName { get; init; } = "";
    public string ItemName { get; init; } = "";

    public Npc? GetOriginNpc(GameAllResponse response) => response.GetRow<Npc>(OriginNpcId);
    public Npc? GetDestinationNpc(GameAllResponse response) => response.GetRow<Npc>(DestinationNpcId);
}

public sealed record BozjaFate : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint Sort { get; init; }
    public uint FateType { get; init; }
    public uint Icon { get; init; }
    public uint MapId { get; init; }
    public uint X { get; init; }
    public uint Y { get; init; }
    public uint? SpawnedById { get; init; }
    public string Name { get; init; } = "";
    public string? SpawnedBy { get; init; }
}

public sealed record BozjaNote : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint Number { get; init; }
    public uint Stars { get; init; }
    public bool Obtainable { get; init; }

    [JsonPropertyName("bozjaFates")]
    public List<uint>? BozjaFateIds { get; init; }

    public string Name { get; init; } = "";
    public string HowTo { get; init; } = "";

    public List<BozjaFate> GetBozjaFates(GameAllResponse response) =>
        BozjaFateIds?.Select(response.GetRow<BozjaFate>).OfType<BozjaFate>().ToList() ?? [];
}

public sealed record PortraitCondition : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint Sort { get; init; }
    public bool Obtainable { get; init; }
    public uint? SourceTypeId { get; init; }
    public uint UnlockType { get; init; }
    public string Name { get; init; } = "";
    public string HowTo { get; init; } = "";

    public SourceType GetSourceType(GameAllResponse response) => response.GetSourceType(SourceTypeId);
}

public sealed record Portrait : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint PortraitConditionId { get; init; }
    public uint Sort { get; init; }
    public uint? Icon { get; init; }
    public uint? Icon2 { get; init; }
    public string Name { get; init; } = "";

    public PortraitCondition? GetPortraitCondition(GameAllResponse response) => response.GetRow<PortraitCondition>(PortraitConditionId);
}

public sealed record TripleTriadCardType : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public string Name { get; init; } = "";
}

public sealed record TripleTriadRule : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public string Name { get; init; } = "";
}

public sealed record TripleTriadCard : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }
    public uint Top { get; init; }
    public uint Left { get; init; }
    public uint Bottom { get; init; }
    public uint Right { get; init; }
    public uint SaleValue { get; init; }
    public uint Sort { get; init; }
    public bool StartsWithVowel { get; init; }
    public uint? SourceTypeId { get; init; }
    public uint Rarity { get; init; }
    public bool? Obtainable { get; init; }

    [JsonPropertyName("ttcardTypeId")]
    public uint TripleTriadCardTypeId { get; init; }

    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string HowTo { get; init; } = "";

    public SourceType GetSourceType(GameAllResponse response) => response.GetSourceType(SourceTypeId);
    public TripleTriadCardType? GetTripleTriadCardType(GameAllResponse response) => response.GetRow<TripleTriadCardType>(TripleTriadCardTypeId);
}

public sealed record TripleTriadNpc : ITable {
    public uint Id { get; init; }
    public long UpdatedAt { get; init; }
    public bool? Deleted { get; init; }

    [JsonPropertyName("fixedTtcardIds")]
    public List<uint>? FixedTripleTriadCardIds { get; init; }

    [JsonPropertyName("variableTtcardIds")]
    public List<uint>? VariableTripleTriadCardIds { get; init; }

    [JsonPropertyName("rewardTtcardIds")]
    public List<uint>? RewardTripleTriadCardIds { get; init; }

    [JsonPropertyName("ttruleIds")]
    public List<uint>? TripleTriadRuleIds { get; init; }

    public bool UsesRegionalRules { get; init; }
    public bool RequireAllPreviousQuest { get; init; }
    public uint Fee { get; init; }
    public uint? NpcId { get; init; }
    public bool Obtainable { get; init; }
    public List<string>? PreviousQuestNames { get; init; }

    public Npc? GetNpc(GameAllResponse response) => NpcId is null ? null : response.GetRow<Npc>(NpcId.Value);

    public List<TripleTriadCard> GetFixedTripleTriadCards(GameAllResponse response) =>
        FixedTripleTriadCardIds?.Select(response.GetRow<TripleTriadCard>).OfType<TripleTriadCard>().ToList() ?? [];

    public List<TripleTriadCard> GetVariableTripleTriadCards(GameAllResponse response) =>
        VariableTripleTriadCardIds?.Select(response.GetRow<TripleTriadCard>).OfType<TripleTriadCard>().ToList() ?? [];

    public List<TripleTriadCard> GetRewardTripleTriadCards(GameAllResponse response) =>
        RewardTripleTriadCardIds?.Select(response.GetRow<TripleTriadCard>).OfType<TripleTriadCard>().ToList() ?? [];

    public List<TripleTriadRule> GetTripleTriadRules(GameAllResponse response) =>
        TripleTriadRuleIds?.Select(response.GetRow<TripleTriadRule>).OfType<TripleTriadRule>().ToList() ?? [];
}
