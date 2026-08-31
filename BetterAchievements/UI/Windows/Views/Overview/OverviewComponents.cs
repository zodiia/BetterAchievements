using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    private readonly record struct PlayerLocation(string Name, string World, string DataCenter);

    private static PlayerLocation GetPlayerLocation() {
        var player = Plugin.ObjectTable.LocalPlayer;
        var name = player?.Name.TextValue ?? "Unknown Adventurer";

        var world = "-";
        var dataCenter = "-";
        if (player is not null && player.HomeWorld.IsValid) {
            var homeWorld = player.HomeWorld.Value;
            world = homeWorld.Name.ToString();
            if (homeWorld.DataCenter.IsValid) {
                dataCenter = homeWorld.DataCenter.Value.Name.ToString();
            }
        }

        return new PlayerLocation(name, world, dataCenter);
    }

    private static void RightAlignedText(Vector2 lineStart, float width, Vector4 color, string text) {
        var textSize = ImGui.CalcTextSize(text);
        ImGui.SetCursorScreenPos(new Vector2(lineStart.X + width - textSize.X, lineStart.Y));
        ImGui.TextColored(color, text);
    }
}
