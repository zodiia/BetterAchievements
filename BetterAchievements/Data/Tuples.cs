using BetterAchievements.Data.Unlockable;

namespace BetterAchievements.Data;

public sealed record NearingCompletionCandidate(double Ratio, UnlockableAchievement Achievement);

public sealed record PointsScore(uint Obtained, uint Total);

public sealed record CategoryWithBreadcrumbs(AchievementLayoutCategory Category, string Breadcrumb);

public sealed record CollectionProgress(PointsScore Score, uint VisibleCount);

public sealed record UnlockableKey(UnlockableType Type, uint Id);
