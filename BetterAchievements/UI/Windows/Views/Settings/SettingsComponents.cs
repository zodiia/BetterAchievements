using System.Numerics;
using BetterAchievements.UI.Component;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace BetterAchievements.UI.Windows.Views.Settings;

public static partial class SettingsComponents {
    private static bool SectionHeader(string name, bool open) {
        ImGui.Dummy(new Vector2(0, UiSize.Em(1)));

        bool clicked;
        using (UiFonts.FontSize125().Push()) {
            var drawList = ImGui.GetWindowDrawList();
            var cursorScreenPos = ImGui.GetCursorScreenPos();
            var avail = ImGui.GetContentRegionAvail().X;
            var textSize = ImGui.CalcTextSize(name);
            var arrowColumnWidth = textSize.Y;

            clicked = ImGui.InvisibleButton("##SectionHeader", new Vector2(avail, textSize.Y));
            if (ImGui.IsItemHovered()) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            var afterHeader = ImGui.GetCursorScreenPos();

            using (ImRaii.PushFont(UiBuilder.IconFont)) {
                var arrow = (open ? FontAwesomeIcon.ChevronDown : FontAwesomeIcon.ChevronRight).ToIconString();
                var arrowSize = ImGui.CalcTextSize(arrow);
                ImGui.SetCursorScreenPos(new Vector2(
                                             cursorScreenPos.X + ((arrowColumnWidth - arrowSize.X) / 2),
                                             cursorScreenPos.Y + ((textSize.Y - arrowSize.Y) / 2)));
                ImGui.Text(arrow);
            }

            var textX = cursorScreenPos.X + arrowColumnWidth + UiSize.Em(0.5f);
            ImGui.SetCursorScreenPos(cursorScreenPos with { X = textX });
            using (UiFonts.FontSize110()) ImGui.Text(name);

            var lineY = cursorScreenPos.Y + (textSize.Y / 2);
            drawList.AddLine(
                new Vector2(textX + textSize.X + UiSize.Em(1), lineY),
                new Vector2(cursorScreenPos.X + avail, lineY),
                ImGui.GetColorU32(ImGuiCol.Separator));

            ImGui.SetCursorScreenPos(afterHeader);
        }

        ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f)));
        return clicked;
    }

    private static void SettingRow(ISetting setting, Configuration draft) {
        using var id = ImRaii.PushId(setting.Name);
        using var disabled = ImRaii.Disabled(setting.IsDisabled?.Invoke(draft) ?? false);
        using var itemWidth = ImRaii.ItemWidth(UiSize.Em(16));

        setting.Draw(draft);
        ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f)));
    }

    private static void Label(ISetting setting) {
        ImGui.TextWrapped(setting.Name);
    }

    private static void Description(ISetting setting) {
        if (setting.Description == null) return;

        using var descriptionColor = ImRaii.PushColor(ImGuiCol.Text, UiColors.Grey());
        ImGui.TextWrapped(setting.Description);
    }

    public static void Section(SettingsSection section, Configuration draft) {
        using var id = ImRaii.PushId(section.Name);

        if (SectionHeader(section.Name, section.IsOpen)) {
            section.IsOpen = !section.IsOpen;
        }

        if (!section.IsOpen) return;

        foreach (var setting in section.Settings) {
            SettingRow(setting, draft);
            ImGui.Dummy(new(0, UiSize.Em(0.5f)));
        }
    }

    public static bool SaveButton(bool disabled, float height) {
        using var disabledScope = ImRaii.Disabled(disabled);
        using (UiFonts.FontSize125().Push()) {
            return ImGui.Button("Save settings", new Vector2(ImGui.GetContentRegionAvail().X, height));
        }
    }
}
