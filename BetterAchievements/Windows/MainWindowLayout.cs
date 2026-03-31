using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BetterAchievements.Windows;

public record MainWindowLayout
{
    public required List<AchievementLayout> AchievementLayout { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutGroup), typeDiscriminator: "group")]
[JsonDerivedType(typeof(AchievementLayoutCategory), typeDiscriminator: "category")]
public abstract record AchievementLayout
{
    public required string Name { get; set; }
}

public record AchievementLayoutGroup : AchievementLayout
{
    public required List<AchievementLayout> Items { get; set; }
}

public record AchievementLayoutCategory : AchievementLayout
{
    public required List<AchievementLayoutItem> Items { get; set; }
    public required uint Id { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutItemSimple), typeDiscriminator: "simple")]
[JsonDerivedType(typeof(AchievementLayoutItemTiered), typeDiscriminator: "tiered")]
[JsonDerivedType(typeof(AchievementLayoutItemCombined), typeDiscriminator: "combined")]
public abstract record AchievementLayoutItem { }

public record AchievementLayoutItemSimple : AchievementLayoutItem
{
    public required uint Id { get; set; }
}

public record AchievementLayoutItemTiered : AchievementLayoutItem
{
    public required List<uint> Ids { get; set; }
}

public record AchievementLayoutItemCombined : AchievementLayoutItem
{
    public required List<uint> Ids { get; set; }
}
