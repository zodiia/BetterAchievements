using BetterAchievements.Data.Unlockable;

namespace BetterAchievements.Data;

public sealed record NearingCompletionCandidate(double Ratio, UnlockableAchievement Achievement);

public sealed record PointsScore(uint Obtained, uint Total);

public sealed record CategoryWithBreadcrumbs(AchievementLayoutCategory Category, string Breadcrumb);
