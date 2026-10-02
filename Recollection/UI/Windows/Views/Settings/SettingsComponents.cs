using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Recollection.UI.Component;

namespace Recollection.UI.Windows.Views.Settings;

public static partial class SettingsComponents {
    private static bool SectionHeader(Plugin plugin, string name, bool open) {
        ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));
        var clicked = DrawHeader(name, open);
        ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));
        return clicked;
    }

    private static bool DrawHeader(string name, bool open) {
        using var font = UiFonts.FontSize125().Push();

        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var height = ImGui.GetTextLineHeight();
        var clicked = ImGui.InvisibleButton("##SectionHeader", new Vector2(width, height));

        if (ImGui.IsItemHovered()) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        var afterHeader = ImGui.GetCursorScreenPos();

        DrawHeaderArrow(origin, height, open);
        var titleEndX = DrawHeaderTitle(origin, height, name);
        DrawHeaderSeparator(origin, width, height, titleEndX);

        ImGui.SetCursorScreenPos(afterHeader);
        return clicked;
    }

    private static void DrawHeaderArrow(Vector2 origin, float columnSize, bool open) {
        using var font = ImRaii.PushFont(UiBuilder.IconFont);

        var arrow = (open ? FontAwesomeIcon.ChevronDown : FontAwesomeIcon.ChevronRight).ToIconString();
        var arrowSize = ImGui.CalcTextSize(arrow);
        ImGui.SetCursorScreenPos(origin + new Vector2((columnSize - arrowSize.X) / 2, (columnSize - arrowSize.Y) / 2));
        ImGui.Text(arrow);
    }

    private static float DrawHeaderTitle(Vector2 origin, float columnSize, string name) {
        var textX = origin.X + columnSize + UiSize.Em(0.5f);

        ImGui.SetCursorScreenPos(origin with { X = textX });
        using (UiFonts.FontSize110()) {
            ImGui.Text(name);
            return textX + ImGui.CalcTextSize(name).X;
        }
    }

    private static void DrawHeaderSeparator(Vector2 origin, float width, float height, float titleEndX) {
        var y = origin.Y + (height / 2);
        ImGui.GetWindowDrawList().AddLine(
            new Vector2(titleEndX + UiSize.Em(1), y),
            new Vector2(origin.X + width, y),
            ImGui.GetColorU32(ImGuiCol.Separator));
    }

    private static void SettingRow(Plugin plugin, ISetting setting, Configuration draft) {
        using var id = ImRaii.PushId(setting.Name);
        using var disabled = ImRaii.Disabled(setting.IsDisabled?.Invoke(draft) ?? false);
        using var itemWidth = ImRaii.ItemWidth(UiSize.Em(16));

        setting.Draw(draft);
        ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));
    }

    private static void Label(ISetting setting) {
        ImGui.TextWrapped(setting.Name);
    }

    private static void Description(ISetting setting) {
        if (setting.Description == null) return;

        using var descriptionColor = ImRaii.PushColor(ImGuiCol.Text, UiColors.Grey());
        ImGui.TextWrapped(setting.Description);
    }

    public static void Section(Plugin plugin, SettingsSection section, Configuration draft) {
        using var id = ImRaii.PushId(section.Name);

        if (SectionHeader(plugin, section.Name, section.IsOpen)) {
            section.IsOpen = !section.IsOpen;
        }

        if (!section.IsOpen) return;

        foreach (var setting in section.Settings) {
            SettingRow(plugin, setting, draft);
            ImGui.Dummy(new(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));
        }
    }

    public static bool SaveButton(bool disabled, float height) {
        using var disabledScope = ImRaii.Disabled(disabled);
        using (UiFonts.FontSize125().Push()) {
            return ImGui.Button("Save settings", new Vector2(ImGui.GetContentRegionAvail().X, height));
        }
    }
}
