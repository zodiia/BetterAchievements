using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BetterAchievements.Data;

public record MainLayout
{
    public required List<AchievementLayout> AchievementLayout { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutGroup), typeDiscriminator: "group")]
[JsonDerivedType(typeof(AchievementLayoutCategory), typeDiscriminator: "category")]
public abstract record AchievementLayout
{
    public required string Name { get; init; }
}

public record AchievementLayoutGroup : AchievementLayout
{
    public required List<AchievementLayout> Items { get; init; }
}

public record AchievementLayoutCategory : AchievementLayout
{
    public required List<AchievementLayoutItem> Items { get; init; }
    public required int Id { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutItemSimple), typeDiscriminator: "simple")]
[JsonDerivedType(typeof(AchievementLayoutItemTiered), typeDiscriminator: "tiered")]
[JsonDerivedType(typeof(AchievementLayoutItemCombined), typeDiscriminator: "combined")]
public abstract record AchievementLayoutItem { }

public record AchievementLayoutItemSimple : AchievementLayoutItem
{
    public required uint Id { get; init; }
}

public record AchievementLayoutItemTiered : AchievementLayoutItem
{
    public required List<uint> Ids { get; init; }
}

public record AchievementLayoutItemCombined : AchievementLayoutItem
{
    public required List<uint> Ids { get; init; }
}
