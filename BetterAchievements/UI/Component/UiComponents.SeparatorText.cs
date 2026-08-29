using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ManagedFontAtlas;

namespace BetterAchievements.UI.Component;

public static partial class UiComponents {
    public static void SeparatorText(string text, Func<IFontHandle>? fontSize = null, Vector4? color = null, float paddingAboveEm = 1f, float paddingBelowEm = 0.5f) {
        ImGui.Dummy(new Vector2(0, UiSize.Em(paddingAboveEm)));
        fontSize ??= UiFonts.FontSize125;

        using (fontSize().Push()) {
            var drawList = ImGui.GetWindowDrawList();
            var cursorScreenPos = ImGui.GetCursorScreenPos();
            var avail = ImGui.GetContentRegionAvail().X;
            var textSize = ImGui.CalcTextSize(text);

            ImGui.TextColored(color ?? UiColors.Text(), text);

            var lineY = cursorScreenPos.Y + (textSize.Y / 2);
            drawList.AddLine(
                new Vector2(cursorScreenPos.X + textSize.X + UiSize.Em(1f), lineY),
                new Vector2(cursorScreenPos.X + avail, lineY),
                ImGui.GetColorU32(ImGuiCol.Separator));
        }

        ImGui.Dummy(new Vector2(0, UiSize.Em(paddingBelowEm)));
    }
}
