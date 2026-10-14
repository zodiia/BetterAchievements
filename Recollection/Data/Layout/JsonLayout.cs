using System.Collections.Generic;

namespace Recollection.Data.Layout;

public record JsonLayout(List<JsonAchievementGroup> AchievementCategories);

public record JsonAchievementGroup(string Name, string? Icon, List<JsonAchievementCategory> Categories);

public record JsonAchievementCategory(string Name, bool? Seasonal, List<JsonAchievementSection> Sections);

public record JsonAchievementSection(string Name, List<JsonAchievementItem> Items);

public record JsonAchievementItem(
    List<uint>? Unique,
    List<uint>? Tiered,
    bool? Spoilers,
    uint? Area,
    uint? HuntingLog
);
