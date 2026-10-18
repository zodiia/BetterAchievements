using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.UI.Component;
using Recollection.UI.State;
using AchievementRoulette = Recollection.UI.State.AchievementRoulette;

namespace Recollection.UI.Windows.Views.Roulette;

public class RouletteView(Plugin plugin, RouletteState roulette) : IView {
    private const string AchievementListNotLoadedWarning = "Achievement list not loaded, please open the vanilla achievement window once!";

    private readonly RouletteRoll roll = new();
    private AchievementRoulette? pendingAbandon;

    private static void DrawCenteredWarning(string text) {
        var available = ImGui.GetContentRegionAvail();
        var textSize = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorPos();

        ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + ((available.X - textSize.X) / 2), Y = cursorPos.Y + ((available.Y - textSize.Y) / 2) });
        ImGui.TextColored(UiColors.Red(), text);
    }

    public void Draw() {
        var ySize = ImGui.GetContentRegionAvail().Y;
        using var mainContent = ImRaii.Child("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true);
        if (!mainContent) return;

        if (!Plugin.UnlockState.IsAchievementListLoaded) {
            DrawCenteredWarning(AchievementListNotLoadedWarning);
            return;
        }

        ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));

        if (RouletteComponents.ActiveRoulettes(plugin, roulette) is { } abandon) {
            pendingAbandon = abandon;
            ImGui.OpenPopup(RouletteComponents.AbandonPopupId);
        }

        RouletteComponents.RollSection(plugin, roulette, roll);
        RouletteComponents.CompletedRoulettes(plugin, roulette);

        if (RouletteComponents.AbandonModal(roulette, pendingAbandon)) {
            pendingAbandon = null;
        }
    }
}
