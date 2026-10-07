using Recollection.Data.Unlockable;

namespace Recollection.Data;

public sealed record AchievementCompletionRatio(UnlockableAchievement Achievement, double Ratio);

public sealed record Score(uint Obtained, uint Total);

public sealed record AchievementProgress(Score Count, Score Progress);

public sealed record CategoryWithBreadcrumbs(AchievementLayoutCategory Category, string Breadcrumb);

public sealed record CollectionProgress(Score Score, uint VisibleCount);

public sealed record UnlockableKey(UnlockableType Type, uint Id);
