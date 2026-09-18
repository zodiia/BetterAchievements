using Recollection.Data.Unlockable;

namespace Recollection.Data;

public sealed record AchievementCompletionRatio(UnlockableAchievement Achievement, double Ratio);

public sealed record PointsScore(uint Obtained, uint Total);

public sealed record CategoryWithBreadcrumbs(AchievementLayoutCategory Category, string Breadcrumb);

public sealed record CollectionProgress(PointsScore Score, uint VisibleCount);

public sealed record UnlockableKey(UnlockableType Type, uint Id);
