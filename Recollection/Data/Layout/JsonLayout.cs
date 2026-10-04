using System.Collections.Generic;

namespace Recollection.Data.Layout;

public record JsonLayout(List<JsonAchievementCategory> AchievementCategories);

public record JsonAchievementCategory(string Name, string? Icon, bool? Seasonal, List<JsonAchievementCategory>? Categories, List<JsonAchievementItem>? Items);

public record JsonAchievementItem(
    List<uint>? Unique,
    List<uint>? Tiered,
    bool? Spoilers,
    uint? Area,
    uint? HuntingLog
);
