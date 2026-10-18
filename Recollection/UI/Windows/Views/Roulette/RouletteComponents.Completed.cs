using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.Helpers;
using Recollection.UI.Component;
using Recollection.UI.State;
using AchievementRoulette = Recollection.UI.State.AchievementRoulette;

namespace Recollection.UI.Windows.Views.Roulette;

public static partial class RouletteComponents {
    public static void CompletedRoulettes(Plugin plugin, RouletteState roulette) {
        var completed = roulette.Completed;
        if (completed.Count == 0) return;

        UiComponents.SectionHeader($"Completed roulettes ({completed.Count})", plugin.Configuration.UiDensity);

        ImGui.Dummy(new Vector2(0, UiSize.Em(0.25f) * plugin.Configuration.UiDensity));

        for (var i = 0; i < completed.Count; i++) {
            using var id = ImRaii.PushId((int)completed[i].Roulette.Id);
            if (i > 0) {
                ImGui.Dummy(new Vector2(0, UiSize.Em(0.75f) * plugin.Configuration.UiDensity));
            }

            CompletedRoulette(completed[i]);
        }
    }

    private static void CompletedRoulette(AchievementRoulette achievementRoulette) {
        var completedAt = achievementRoulette.CompletedAt!.Value;
        var drawList = ImGui.GetWindowDrawList();
        var accentWidth = UiSize.Em(0.2f);
        var accentStart = ImGui.GetCursorScreenPos();

        ImGui.Indent(UiSize.Em(0.75f));
        ImGui.BeginGroup();

        RouletteHeader(achievementRoulette);

        using (var table = ImRaii.Table("Details", 2)) {
            if (table) {
                ImGui.TableSetupColumn("##Key", ImGuiTableColumnFlags.WidthFixed, UiSize.Em(10f));
                ImGui.TableSetupColumn("##Value", ImGuiTableColumnFlags.WidthStretch);

                KeyValue("Rolled on", achievementRoulette.RolledAtText, UiColors.Text(), $"({TimeFormat.FormatTimeAgo(achievementRoulette.RolledAt)})");
                KeyValue("Completed on", achievementRoulette.CompletedAtText!, UiColors.Text(), $"({TimeFormat.FormatTimeAgo(completedAt)})");

                if (achievementRoulette.Maximum > 1) {
                    ImGui.TableNextColumn();
                    ImGui.TextColored(UiColors.Grey(), "Progress");
                    ImGui.TableNextColumn();
                    Graph(achievementRoulette, UiColors.Progress());
                } else {
                    KeyValue("Progress", "No progress counter", UiColors.Grey());
                }
            }
        }

        ImGui.Dummy(new Vector2(0, UiSize.Em(0.25f)));
        ShareButton(achievementRoulette);

        ImGui.EndGroup();
        ImGui.Unindent(UiSize.Em(0.75f));

        var accentEnd = ImGui.GetItemRectMax();
        drawList.AddRectFilled(accentStart, new Vector2(accentStart.X + accentWidth, accentEnd.Y), ImGui.GetColorU32(UiColors.Progress()), accentWidth / 2);
    }
}
