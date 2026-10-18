using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.Helpers;
using Recollection.Services;
using Recollection.UI.Component;
using Recollection.UI.State;
using AchievementRoulette = Recollection.UI.State.AchievementRoulette;

namespace Recollection.UI.Windows.Views.Roulette;

public static partial class RouletteComponents {
    public static AchievementRoulette? ActiveRoulettes(Plugin plugin, RouletteState roulette) {
        var active = roulette.Active;
        AchievementRoulette? abandon = null;

        UiComponents.SectionHeader($"Active roulettes ({active.Count})", plugin.Configuration.UiDensity);

        if (active.Count == 0) {
            ImGui.TextColored(UiColors.Grey(), "No active roulettes, roll a new achievement!");
            return null;
        }

        for (var i = 0; i < active.Count; i++) {
            using var id = ImRaii.PushId((int)active[i].Roulette.Id);
            if (i > 0) {
                ImGui.Dummy(new Vector2(0, UiSize.Em(0.75f) * plugin.Configuration.UiDensity));
            }

            if (ActiveRoulette(plugin, active[i])) {
                abandon = active[i];
            }
        }

        return abandon;
    }

    private static bool ActiveRoulette(Plugin plugin, AchievementRoulette achievementRoulette) {
        var drawList = ImGui.GetWindowDrawList();
        var accentWidth = UiSize.Em(0.2f);
        var accentStart = ImGui.GetCursorScreenPos();

        ImGui.Indent(UiSize.Em(0.75f));
        ImGui.BeginGroup();

        RouletteHeader(achievementRoulette);
        if (achievementRoulette.Maximum > 1) {
            ActiveRouletteProgressBar(plugin, achievementRoulette);
        }

        ActiveRouletteDetails(achievementRoulette);
        ImGui.Dummy(new Vector2(0, UiSize.Em(0.25f)));
        var abandon = ActiveRouletteButtons(achievementRoulette);

        ImGui.EndGroup();
        ImGui.Unindent(UiSize.Em(0.75f));

        var accentEnd = ImGui.GetItemRectMax();
        drawList.AddRectFilled(accentStart, accentEnd with { X = accentStart.X + accentWidth }, ImGui.GetColorU32(UiColors.Progress()), accentWidth / 2);
        return abandon;
    }

    private static void ActiveRouletteProgressBar(Plugin plugin, AchievementRoulette achievementRoulette) {
        var current = achievementRoulette.Current;
        var maximum = achievementRoulette.Maximum;

        UiComponents.ProgressBar(
            (float)current / maximum,
            UiColors.Progress(),
            height: UiSize.Em(plugin.Configuration.ProgressBarHeight),
            insideText: $"{current:N0}/{maximum:N0}",
            tooltip: "Click to refresh",
            onClick: () => AchievementProgressService.OpenAchievementWindow(achievementRoulette.Achievement.RowId));
    }

    private static void ActiveRouletteDetails(AchievementRoulette achievementRoulette) {
        using var table = ImRaii.Table("Details", 2);
        if (!table) return;

        var maximum = achievementRoulette.Maximum;
        var current = achievementRoulette.Current;
        var gained = current > achievementRoulette.ProgressAtRoll ? current - achievementRoulette.ProgressAtRoll : 0;

        ImGui.TableSetupColumn("##Key", ImGuiTableColumnFlags.WidthFixed, UiSize.Em(10f));
        ImGui.TableSetupColumn("##Value", ImGuiTableColumnFlags.WidthStretch);

        KeyValue("Rolled on", achievementRoulette.RolledAtText, UiColors.Text(), $"({TimeFormat.FormatTimeAgo(achievementRoulette.RolledAt)})");

        if (maximum > 1) {
            KeyValue("Since rolled", $"+{gained:N0}", UiColors.Blue(), $"(+{(float)gained / maximum * 100:0.#}%)");

            ImGui.TableNextColumn();
            ImGui.TextColored(UiColors.Grey(), "Progress");
            ImGui.TableNextColumn();
            Graph(achievementRoulette, UiColors.Progress());
        } else {
            KeyValue("Progress", "No progress counter", UiColors.Grey());
        }
    }

    private static bool ActiveRouletteButtons(AchievementRoulette achievementRoulette) {
        ShareButton(achievementRoulette);
        ImGui.SameLine();

        using (ImRaii.PushColor(ImGuiCol.Button, UiColors.Red() with { W = 0.6f })
                     .Push(ImGuiCol.ButtonHovered, UiColors.Red() with { W = 0.8f })
                     .Push(ImGuiCol.ButtonActive, UiColors.Red())) {
            var abandon = ImGui.Button("Abandon");
            if (ImGui.IsItemHovered()) {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            return abandon;
        }
    }
}
