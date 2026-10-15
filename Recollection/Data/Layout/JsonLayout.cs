using System.Collections.Generic;

namespace Recollection.Data.Layout;

public record JsonLayout(List<JsonAchievementGroup> AchievementCategories);

public record JsonAchievementGroup(string Name, string? Icon, List<JsonAchievementCategory> Categories);

public record JsonAchievementCategory(string Name, List<JsonAchievementSection> Sections);

public record JsonAchievementSection(string Name, bool? Seasonal, List<JsonAchievementItem> Items);

public record JsonAchievementItem(
    List<uint>? Unique,
    List<uint>? Tiered,
    bool? Spoilers,
    List<uint>? Area,
    uint? HuntingLog,
    uint? RequiredQuest
);
