using BetterAchievements.Windows.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using ImGuiComponentsDalamud = Dalamud.Interface.Components.ImGuiComponents;


namespace BetterAchievements.Windows.Components;

public static partial class ImGuiComponents
{
    public const string FiltersPopupId = "FilterPopup";

    public static void FiltersPopup(MainWindowState state)
    {
        using (var popup = ImRaii.Popup(FiltersPopupId, ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (!popup) return;

            ImGui.Text("Filters");

            using (var combo = ImRaii.Combo("Unlock status", state.UnlockStatusFilter.DisplayName(), ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    if (ImGui.Selectable(UnlockStatusFilter.All.DisplayName(), state.UnlockStatusFilter == UnlockStatusFilter.All))
                        state.SetUnlockStatusFilter(UnlockStatusFilter.All);
                    if (ImGui.Selectable(UnlockStatusFilter.Unlocked.DisplayName(), state.UnlockStatusFilter == UnlockStatusFilter.Unlocked))
                        state.SetUnlockStatusFilter(UnlockStatusFilter.Unlocked);
                    if (ImGui.Selectable(UnlockStatusFilter.Locked.DisplayName(), state.UnlockStatusFilter == UnlockStatusFilter.Locked))
                        state.SetUnlockStatusFilter(UnlockStatusFilter.Locked);
                }
            }
            using (var combo = ImRaii.Combo("Contains rewards (title, minion, ...)", "(not implemented)", ImGuiComboFlags.HeightLargest))
            // using (var combo = ImRaii.Combo("Contains rewards (title, minion, ...)", state.ContainsRewardsFilter.DisplayName(), ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    if (ImGui.Selectable(ContainsRewardsFilter.All.DisplayName(), state.ContainsRewardsFilter == ContainsRewardsFilter.All))
                        state.SetContainsRewardsFilter(ContainsRewardsFilter.All);
                    if (ImGui.Selectable(ContainsRewardsFilter.Rewards.DisplayName(), state.ContainsRewardsFilter == ContainsRewardsFilter.Rewards))
                        state.SetContainsRewardsFilter(ContainsRewardsFilter.Rewards);
                    if (ImGui.Selectable(ContainsRewardsFilter.UnclaimedRewards.DisplayName(), state.ContainsRewardsFilter == ContainsRewardsFilter.UnclaimedRewards))
                        state.SetContainsRewardsFilter(ContainsRewardsFilter.UnclaimedRewards);
                }
            }
            using (var combo = ImRaii.Combo("Counts towards rankings", state.RankedFilter.DisplayName(), ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    if (ImGui.Selectable(RankedFilter.All.DisplayName(), state.RankedFilter == RankedFilter.All))
                        state.SetRankedFilter(RankedFilter.All);
                    if (ImGui.Selectable(RankedFilter.Lalachievements.DisplayName(), state.RankedFilter == RankedFilter.Lalachievements))
                        state.SetRankedFilter(RankedFilter.Lalachievements);
                }
            }
            using (var combo = ImRaii.Combo("Area", "(not implemented)", ImGuiComboFlags.HeightLargest))
            // using (var combo = ImRaii.Combo("Area", state.AreaFilter.DisplayName(), ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    if (ImGui.Selectable(AreaFilter.All.DisplayName(), state.AreaFilter == AreaFilter.All))
                        state.SetAreaFilter(AreaFilter.All);
                    if (ImGui.Selectable(AreaFilter.Region.DisplayName(), state.AreaFilter == AreaFilter.Region))
                        state.SetAreaFilter(AreaFilter.Region);
                    if (ImGui.Selectable(AreaFilter.Zone.DisplayName(), state.AreaFilter == AreaFilter.Zone))
                        state.SetAreaFilter(AreaFilter.Zone);
                }
            }

            ImGui.Text("Sorting options");

            using (var combo = ImRaii.Combo("Sort by", state.SortBy.DisplayName(), ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    if (ImGui.Selectable(SortBy.Default.DisplayName(), state.SortBy == SortBy.Default))
                        state.SetSortBy(SortBy.Default);
                    if (ImGui.Selectable(SortBy.Alphabetically.DisplayName(), state.SortBy == SortBy.Alphabetically))
                        state.SetSortBy(SortBy.Alphabetically);
                    if (ImGui.Selectable(SortBy.MostCommon.DisplayName(), state.SortBy == SortBy.MostCommon))
                        state.SetSortBy(SortBy.MostCommon);
                    if (ImGui.Selectable(SortBy.Rarest.DisplayName(), state.SortBy == SortBy.Rarest))
                        state.SetSortBy(SortBy.Rarest);
                }
            }

            // using (var combo = ImRaii.Combo("Group achievements by", state.GroupBy.DisplayName(), ImGuiComboFlags.HeightLargest))
            using (var combo = ImRaii.Combo("Group achievements by", "(not implemented)", ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    if (ImGui.Selectable(GroupBy.Default.DisplayName(), state.GroupBy == GroupBy.Default))
                        state.SetGroupBy(GroupBy.Default);
                    if (ImGui.Selectable(GroupBy.Better.DisplayName(), state.GroupBy == GroupBy.Better))
                        state.SetGroupBy(GroupBy.Better);
                }
            }
        }
    }
}
