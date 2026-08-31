using BetterAchievements.UI.Component;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    public static void PlayerHeader() {
        var location = GetPlayerLocation();
        var lineStartY = ImGui.GetCursorPosY();

        using (UiFonts.FontSize150().Push()) {
            ImGui.TextColored(UiColors.Text(), location.Name);
        }

        ImGui.SameLine(0, UiSize.Em(1f));
        ImGui.SetCursorPosY(lineStartY + UiSize.Em(0.5f));
        using (UiFonts.FontSize110().Push()) {
            ImGui.TextColored(UiColors.Grey(), $"{location.World}, {location.DataCenter}");
        }
    }
}
