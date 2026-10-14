using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility.Raii;

namespace Recollection.UI.Component;

public static partial class UiComponents {
    public static bool SectionHeader(string text, float uiDensity, Func<IFontHandle>? fontSize = null, Vector4? color = null, float paddingAboveEm = 1f, float paddingBelowEm = 0.5f, bool? open = null) {
        ImGui.Dummy(new Vector2(0, UiSize.Em(paddingAboveEm * uiDensity)));
        fontSize ??= UiFonts.FontSize125;
        var clicked = false;

        using (fontSize().Push()) {
            var drawList = ImGui.GetWindowDrawList();
            var cursorScreenPos = ImGui.GetCursorScreenPos();
            var avail = ImGui.GetContentRegionAvail().X;
            var textSize = ImGui.CalcTextSize(text);

            ImGui.BeginGroup();
            if (open != null) {
                using (ImRaii.PushFont(UiBuilder.IconFont)) {
                    ImGui.TextColored(color ?? UiColors.Text(), (open.Value ? FontAwesomeIcon.ChevronDown : FontAwesomeIcon.ChevronRight).ToIconString());
                }

                ImGui.SameLine();
            }

            ImGui.TextColored(color ?? UiColors.Text(), text);
            ImGui.EndGroup();

            if (open != null) {
                clicked = ImGui.IsItemClicked();
                if (ImGui.IsItemHovered()) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            var lineY = cursorScreenPos.Y + (textSize.Y / 2);
            drawList.AddLine(
                new Vector2(ImGui.GetItemRectMax().X + UiSize.Em(1f), lineY),
                new Vector2(cursorScreenPos.X + avail, lineY),
                ImGui.GetColorU32(ImGuiCol.Separator));
        }

        ImGui.Dummy(new Vector2(0, UiSize.Em(paddingBelowEm * uiDensity)));
        return clicked;
    }
}
