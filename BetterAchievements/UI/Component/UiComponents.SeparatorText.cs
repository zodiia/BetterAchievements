using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents {
    public static void SeparatorText(string text) {
        ImGui.Dummy(new Vector2(0, UiSize.Em(1f)));

        using (UiFonts.FontSize125().Push()) {
            var drawList = ImGui.GetWindowDrawList();
            var cursorScreenPos = ImGui.GetCursorScreenPos();
            var avail = ImGui.GetContentRegionAvail().X;
            var textSize = ImGui.CalcTextSize(text);

            ImGui.TextUnformatted(text);

            var lineY = cursorScreenPos.Y + (textSize.Y / 2);
            drawList.AddLine(
                new Vector2(cursorScreenPos.X + textSize.X + UiSize.Em(1f), lineY),
                new Vector2(cursorScreenPos.X + avail, lineY),
                ImGui.GetColorU32(ImGuiCol.Separator));
        }

        ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f)));
    }
}
