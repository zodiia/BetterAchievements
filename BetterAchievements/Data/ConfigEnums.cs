using System;

namespace BetterAchievements.Data;

public enum UnlockStatusFilter {
    All,
    Unlocked,
    Locked
}

public enum ContainsRewardsFilter {
    All,
    Rewards,
    UnclaimedRewards
}

public enum RankedFilter {
    All,
    Lalachievements
}

public enum AreaFilter {
    All,
    Region,
    Zone
}

public enum SortBy {
    Default,
    Alphabetically,
    MostCommon,
    Rarest
}

public static class ConfigEnumsExtensions {
    extension(UnlockStatusFilter filter) {
        public string DisplayName() => filter switch {
            UnlockStatusFilter.All => "All",
            UnlockStatusFilter.Locked => "Only locked",
            UnlockStatusFilter.Unlocked => "Only unlocked",
            _ => throw new ArgumentOutOfRangeException($"{filter} not implemented.")
        };

        public string DisplayDescription() => filter switch {
            UnlockStatusFilter.All => "Displays all items regardless of their unlock status",
            UnlockStatusFilter.Locked => "Only displays items that have not been unlocked yet",
            UnlockStatusFilter.Unlocked => "Only displays items that have already been unlocked",
            _ => throw new ArgumentOutOfRangeException($"{filter} not implemented.")
        };
    }

    public static string DisplayName(this ContainsRewardsFilter filter) => filter switch {
        ContainsRewardsFilter.All => "All",
        ContainsRewardsFilter.Rewards => "Only with rewards",
        ContainsRewardsFilter.UnclaimedRewards => "Only with unclaimed rewards",
        _ => throw new ArgumentOutOfRangeException($"{filter} not implemented.")
    };

    extension(RankedFilter filter) {
        public string DisplayName() => filter switch {
            RankedFilter.All => "Show all",
            RankedFilter.Lalachievements => "Yes (Lalachievements)",
            _ => throw new ArgumentOutOfRangeException($"{filter} not implemented.")
        };

        public string DisplayDescription() => filter switch {
            RankedFilter.All => "Displays all items regardless of if they count towards rankings or not",
            RankedFilter.Lalachievements => "Only displays items that counts towards rankings and leaderboards on Lalachievements",
            _ => throw new ArgumentOutOfRangeException($"{filter} not implemented.")
        };
    }

    public static string DisplayName(this AreaFilter filter) => filter switch {
        AreaFilter.All => "All",
        AreaFilter.Region => "Current region only",
        AreaFilter.Zone => "Current area only",
        _ => throw new ArgumentOutOfRangeException($"{filter} not implemented.")
    };

    extension(SortBy filter) {
        public string DisplayName() => filter switch {
            SortBy.Default => "Default order",
            SortBy.Alphabetically => "Alphabetical order",
            SortBy.MostCommon => "Most common first",
            SortBy.Rarest => "Rarest first",
            _ => throw new ArgumentOutOfRangeException($"{filter} not implemented.")
        };

        public string DisplayDescription() => filter switch {
            SortBy.Default => "Keeps the original sorting method (for most items, it is the order in which they also appear in vanilla interfaces)",
            SortBy.Alphabetically => "Sorts all items alphabetically in ascending order",
            SortBy.MostCommon => "Puts the most commonly obtained items first (note: currently only works with achievements)",
            SortBy.Rarest => "Puts the rarest items first (note: currently only works with achievements)",
            _ => throw new ArgumentOutOfRangeException($"{filter} not implemented.")
        };
    }
}
