using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BetterAchievements.Windows;

public class MainWindowLayout
{
    public required List<AchievementLayout> AchievementLayout { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutGroup), typeDiscriminator: "group")]
[JsonDerivedType(typeof(AchievementLayoutCategory), typeDiscriminator: "category")]
public abstract class AchievementLayout
{
    public required string Name { get; set; }
}

public class AchievementLayoutGroup : AchievementLayout
{
    public required List<AchievementLayout> Items { get; set; }
}

public class AchievementLayoutCategory : AchievementLayout
{
    public required List<AchievementLayoutItem> Items { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AchievementLayoutItemSimple), typeDiscriminator: "simple")]
[JsonDerivedType(typeof(AchievementLayoutItemTiered), typeDiscriminator: "tiered")]
[JsonDerivedType(typeof(AchievementLayoutItemCombined), typeDiscriminator: "combined")]
public abstract class AchievementLayoutItem { }

public class AchievementLayoutItemSimple : AchievementLayoutItem
{
    public required uint Id { get; set; }
}

public class AchievementLayoutItemTiered : AchievementLayoutItem
{
    public required List<uint> Ids { get; set; }
}

public class AchievementLayoutItemCombined : AchievementLayoutItem
{
    public required List<uint> Ids { get; set; }
}
