using System;

namespace BetterAchievements.Data;

public enum UnlockStatusFilter { All, Unlocked, Locked }
public enum ContainsRewardsFilter { All, Rewards, UnclaimedRewards }
public enum RankedFilter { All, Lalachievements }
public enum AreaFilter { All, Region, Zone }
public enum SortBy { Default, Alphabetically, MostCommon, Rarest }
public enum GroupBy { Default, Better }

public static class FilterEnumsExtensions {
    public static string DisplayName(this UnlockStatusFilter filter)
    {
        switch (filter)
        {
            case UnlockStatusFilter.All: return "All";
            case UnlockStatusFilter.Locked: return "Only locked";
            case UnlockStatusFilter.Unlocked: return "Only unlocked";
        }

        throw new ArgumentOutOfRangeException($"{filter} not implemented.");
    }

    public static string DisplayName(this ContainsRewardsFilter filter)
    {
        switch (filter)
        {
            case ContainsRewardsFilter.All: return "All";
            case ContainsRewardsFilter.Rewards: return "Only with rewards";
            case ContainsRewardsFilter.UnclaimedRewards: return "Only with unclaimed rewards";
        }

        throw new ArgumentOutOfRangeException($"{filter} not implemented.");
    }

    public static string DisplayName(this RankedFilter filter)
    {
        switch (filter)
        {
            case RankedFilter.All: return "All";
            case RankedFilter.Lalachievements: return "Yes (Lalachievements)";
        }

        throw new ArgumentOutOfRangeException($"{filter} not implemented.");
    }

    public static string DisplayName(this AreaFilter filter)
    {
        switch (filter)
        {
            case AreaFilter.All: return "All";
            case AreaFilter.Region: return "Current region only";
            case AreaFilter.Zone: return "Current area only";
        }

        throw new ArgumentOutOfRangeException($"{filter} not implemented.");
    }

    public static string DisplayName(this SortBy filter)
    {
        switch (filter)
        {
            case SortBy.Default: return "Default order";
            case SortBy.Alphabetically: return "Alphabetical order";
            case SortBy.MostCommon: return "Most common first";
            case SortBy.Rarest: return "Rarest first";
        }

        throw new ArgumentOutOfRangeException($"{filter} not implemented.");
    }

    public static string DisplayName(this GroupBy filter)
    {
        switch (filter)
        {
            case GroupBy.Default: return "Vanilla groups";
            case GroupBy.Better: return "Better groups";
        }
        
        throw new ArgumentOutOfRangeException($"{filter} not implemented.");
    }
}
