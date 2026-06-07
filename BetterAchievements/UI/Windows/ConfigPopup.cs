using BetterAchievements.Data;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Windows;

public static class ConfigPopup
{
    public const string FiltersPopupId = "FilterPopup";

    public static void FiltersPopup(MainWindowState state)
    {
        using var popup = ImRaii.Popup(FiltersPopupId, ImGuiWindowFlags.AlwaysAutoResize);
        if (!popup) return;

        ImGui.Text("Filters");

        using (var combo = ImRaii.Combo("Unlock status", state.Configuration.UnlockStatusFilter.DisplayName(), ImGuiComboFlags.HeightLargest))
        {
            if (combo)
            {
                if (ImGui.Selectable(UnlockStatusFilter.All.DisplayName(), state.Configuration.UnlockStatusFilter == UnlockStatusFilter.All))
                    state.SetUnlockStatusFilter(UnlockStatusFilter.All);
                if (ImGui.Selectable(UnlockStatusFilter.Unlocked.DisplayName(), state.Configuration.UnlockStatusFilter == UnlockStatusFilter.Unlocked))
                    state.SetUnlockStatusFilter(UnlockStatusFilter.Unlocked);
                if (ImGui.Selectable(UnlockStatusFilter.Locked.DisplayName(), state.Configuration.UnlockStatusFilter == UnlockStatusFilter.Locked))
                    state.SetUnlockStatusFilter(UnlockStatusFilter.Locked);
            }
        }
        using (var combo = ImRaii.Combo("Contains rewards (title, minion, ...)", "(not implemented)", ImGuiComboFlags.HeightLargest))
            // using (var combo = ImRaii.Combo("Contains rewards (title, minion, ...)", state.Configuration.ContainsRewardsFilter.DisplayName(), ImGuiComboFlags.HeightLargest))
        {
            if (combo)
            {
                if (ImGui.Selectable(ContainsRewardsFilter.All.DisplayName(), state.Configuration.ContainsRewardsFilter == ContainsRewardsFilter.All))
                    state.SetContainsRewardsFilter(ContainsRewardsFilter.All);
                if (ImGui.Selectable(ContainsRewardsFilter.Rewards.DisplayName(), state.Configuration.ContainsRewardsFilter == ContainsRewardsFilter.Rewards))
                    state.SetContainsRewardsFilter(ContainsRewardsFilter.Rewards);
                if (ImGui.Selectable(ContainsRewardsFilter.UnclaimedRewards.DisplayName(), state.Configuration.ContainsRewardsFilter == ContainsRewardsFilter.UnclaimedRewards))
                    state.SetContainsRewardsFilter(ContainsRewardsFilter.UnclaimedRewards);
            }
        }
        using (var combo = ImRaii.Combo("Counts towards rankings", state.Configuration.RankedFilter.DisplayName(), ImGuiComboFlags.HeightLargest))
        {
            if (combo)
            {
                if (ImGui.Selectable(RankedFilter.All.DisplayName(), state.Configuration.RankedFilter == RankedFilter.All))
                    state.SetRankedFilter(RankedFilter.All);
                if (ImGui.Selectable(RankedFilter.Lalachievements.DisplayName(), state.Configuration.RankedFilter == RankedFilter.Lalachievements))
                    state.SetRankedFilter(RankedFilter.Lalachievements);
            }
        }
        using (var combo = ImRaii.Combo("Area", "(not implemented)", ImGuiComboFlags.HeightLargest))
            // using (var combo = ImRaii.Combo("Area", state.Configuration.AreaFilter.DisplayName(), ImGuiComboFlags.HeightLargest))
        {
            if (combo)
            {
                if (ImGui.Selectable(AreaFilter.All.DisplayName(), state.Configuration.AreaFilter == AreaFilter.All))
                    state.SetAreaFilter(AreaFilter.All);
                if (ImGui.Selectable(AreaFilter.Region.DisplayName(), state.Configuration.AreaFilter == AreaFilter.Region))
                    state.SetAreaFilter(AreaFilter.Region);
                if (ImGui.Selectable(AreaFilter.Zone.DisplayName(), state.Configuration.AreaFilter == AreaFilter.Zone))
                    state.SetAreaFilter(AreaFilter.Zone);
            }
        }

        ImGui.Text("Sorting options");

        using (var combo = ImRaii.Combo("Sort by", state.Configuration.SortBy.DisplayName(), ImGuiComboFlags.HeightLargest))
        {
            if (combo)
            {
                if (ImGui.Selectable(SortBy.Default.DisplayName(), state.Configuration.SortBy == SortBy.Default))
                    state.SetSortBy(SortBy.Default);
                if (ImGui.Selectable(SortBy.Alphabetically.DisplayName(), state.Configuration.SortBy == SortBy.Alphabetically))
                    state.SetSortBy(SortBy.Alphabetically);
                if (ImGui.Selectable(SortBy.MostCommon.DisplayName(), state.Configuration.SortBy == SortBy.MostCommon))
                    state.SetSortBy(SortBy.MostCommon);
                if (ImGui.Selectable(SortBy.Rarest.DisplayName(), state.Configuration.SortBy == SortBy.Rarest))
                    state.SetSortBy(SortBy.Rarest);
            }
        }

        // using (var combo = ImRaii.Combo("Group achievements by", state.Configuration.GroupBy.DisplayName(), ImGuiComboFlags.HeightLargest))
        using (var combo = ImRaii.Combo("Group achievements by", "(not implemented)", ImGuiComboFlags.HeightLargest))
        {
            if (combo)
            {
                if (ImGui.Selectable(GroupBy.Default.DisplayName(), state.Configuration.GroupBy == GroupBy.Default))
                    state.SetGroupBy(GroupBy.Default);
                if (ImGui.Selectable(GroupBy.Better.DisplayName(), state.Configuration.GroupBy == GroupBy.Better))
                    state.SetGroupBy(GroupBy.Better);
            }
        }

        ImGui.Text("Other settings");
        
        var displayIds = state.Configuration.DisplayIds;
        if (ImGui.Checkbox("Display IDs", ref displayIds))
        {
            state.Configuration.DisplayIds = displayIds;
            state.Configuration.Save();
        }

        var neverHideProgressBars = state.Configuration.NeverHideProgressBars;
        if (ImGui.Checkbox("Never hide progress bars", ref neverHideProgressBars))
        {
            state.Configuration.NeverHideProgressBars = neverHideProgressBars;
            state.Configuration.Save();
        }
    }
}
