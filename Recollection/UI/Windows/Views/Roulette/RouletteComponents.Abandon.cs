using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.UI.Component;
using Recollection.UI.State;
using AchievementRoulette = Recollection.UI.State.AchievementRoulette;

namespace Recollection.UI.Windows.Views.Roulette;

public static partial class RouletteComponents {
    public const string AbandonPopupId = "Abandon roulette?##AbandonRoulette";

    public static bool AbandonModal(RouletteState roulette, AchievementRoulette? pendingAbandon) {
        var open = true;

        ImGui.SetNextWindowPos(ImGui.GetWindowPos() + (ImGui.GetWindowSize() / 2), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        using var popupBackground = ImRaii.PushColor(ImGuiCol.PopupBg, UiColors.PopupBackground() with { W = 1f });
        if (!ImGui.BeginPopupModal(AbandonPopupId, ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            return !open;
        }

        if (pendingAbandon != null) {
            ImGui.TextUnformatted("Do you really want to abandon this roulette?");
            using (UiFonts.FontSize110().Push()) ImGui.TextColored(UiColors.Progress(), pendingAbandon.Achievement.Name.ToString());
            ImGui.Dummy(new Vector2(0, UiSize.Em(0.25f)));
            ImGui.TextColored(UiColors.Red(), "This cannot be cancelled.");
            ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f)));

            using (ImRaii.PushColor(ImGuiCol.Button, UiColors.Red() with { W = 0.6f })
                         .Push(ImGuiCol.ButtonHovered, UiColors.Red() with { W = 0.8f })
                         .Push(ImGuiCol.ButtonActive, UiColors.Red())) {
                if (ImGui.Button("Abandon")) {
                    roulette.Abandon(pendingAbandon);
                    open = false;
                }

                if (ImGui.IsItemHovered()) {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel")) {
                open = false;
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
        } else {
            open = false;
        }

        if (!open) {
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
        return !open;
    }
}
