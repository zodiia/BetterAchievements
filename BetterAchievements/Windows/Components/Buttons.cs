using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using ImGuiComponentsDalamud = Dalamud.Interface.Components.ImGuiComponents;

namespace BetterAchievements.Windows.Components;

public static partial class ImGuiComponents
{
    private const string DisableFilter = "Disable this filter";
    private const string FilterUnlocked = "Filter only unlocked";
    private const string FilterLocked = "Filter only locked";
    private const string FilterHasRewards = "Filter only achievements that have rewards";
    private const string FilterHasNoReward = "Filter only achievements without rewards";
    private const string FilterRanked = "Filter only achievements that count towards rankings (Lalachievements, ...)";
    private const string FilterNotRanked = "Filter only achievements that are not ranked";
    private const string FilterCurrentZone = "Filter achievements in the current zone";

    private static bool FilterButton(FontAwesomeIcon icon, Vector4 baseColor, string tooltip)
    {
        var clicked = ImGuiComponentsDalamud.IconButton(icon, baseColor.Brightness(-0.45f), baseColor.Brightness(-0.05f), baseColor.Brightness(-0.25f));
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }

        return clicked;
    }

    // public static bool FilterUnlockedButton(TriState state)
    // {
    //     switch (state)
    //     {
    //         case TriState.False:
    //             return
    //     }
    // }
}
