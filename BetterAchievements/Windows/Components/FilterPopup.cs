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

            using (var combo = ImRaii.Combo("Unlock status", "Only unlocked", ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    ImGui.Selectable("All", false);
                    ImGui.Selectable("Only unlocked", true);
                    ImGui.Selectable("Only locked", false);
                }
            }
            using (var combo = ImRaii.Combo("Contains rewards (title, minion, ...)", "Only with rewards", ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    ImGui.Selectable("All", false);
                    ImGui.Selectable("Only with rewards", true);
                    ImGui.Selectable("Only with unclaimed rewards", false);
                }
            }
            using (var combo = ImRaii.Combo("Counts towards rankings", "Yes (Lalachievements)", ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    ImGui.Selectable("All", false);
                    ImGui.Selectable("Yes (Lalachievements)", true);
                }
            }
            using (var combo = ImRaii.Combo("Area", "All", ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    ImGui.Selectable("All", true);
                    ImGui.Selectable("Current region only", false);
                    ImGui.Selectable("Current area only", false);
                }
            }

            ImGui.Text("Sorting options");

            using (var combo = ImRaii.Combo("Sort by", "Default order", ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    ImGui.Selectable("Default order", true);
                    ImGui.Selectable("Alphabetically", false);
                    ImGui.Selectable("Most common first", false);
                    ImGui.Selectable("Rarest first", false);
                }
            }

            using (var combo = ImRaii.Combo("Group achievements by", "Vanilla groups", ImGuiComboFlags.HeightLargest))
            {
                if (combo)
                {
                    ImGui.Selectable("Vanilla groups", true);
                    ImGui.Selectable("Better groups", false);
                }
            }
        }
    }
}
