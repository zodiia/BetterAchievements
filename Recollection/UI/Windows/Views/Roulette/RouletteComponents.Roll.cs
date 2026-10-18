using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.UI.Component;
using Recollection.UI.State;

namespace Recollection.UI.Windows.Views.Roulette;

public static partial class RouletteComponents {
    private static readonly int[] PointSteps = [0, 5, 10, 20, 30, 50];

    public static void RollSection(Plugin plugin, RouletteState roulette, RouletteRoll roll) {
        var buttonHeight = UiSize.Em(2.5f);
        var candidateCount = roulette.RouletteCandidates.Count;

        if (!roll.IsOpen) {
            ImGui.Dummy(new Vector2(0, UiSize.Em(0.75f) * plugin.Configuration.UiDensity));
            using (UiFonts.FontSize125().Push()) {
                if (ImGui.Button("Roll new achievement", new Vector2(ImGui.GetContentRegionAvail().X, buttonHeight))) {
                    roll.IsOpen = true;
                }

                if (ImGui.IsItemHovered()) {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
            }

            return;
        }

        UiComponents.SectionHeader("Roll new achievement", plugin.Configuration.UiDensity);

        if (roll.RerollStartedAt >= 0 && ImGui.GetTime() - roll.RerollStartedAt >= CardRevealDuration) {
            roll.RerollStartedAt = -1;
            roll.Draw(roulette.RouletteCandidates, plugin.Configuration.RouletteCardCount);
        }

        if (roll.Cards.Count > 0) {
            Cards(roulette, roll);
            ImGui.Dummy(new Vector2(0, UiSize.Em(0.25f) * plugin.Configuration.UiDensity));
        } else {
            RollSettings(plugin, roulette, roll);
            ImGui.Dummy(new Vector2(0, UiSize.Em(0.5f) * plugin.Configuration.UiDensity));
        }

        using (UiFonts.FontSize125().Push()) {
            var cardCount = plugin.Configuration.RouletteCardCount;
            var mainLabel = roll.Cards.Count > 0 ? "Reroll" : cardCount == 1 ? "Draw a card" : $"Draw {cardCount} cards";
            var mainWidth = MathF.Max(UiSize.Em(10f), ImGui.CalcTextSize(mainLabel).X + UiSize.Em(3f));
            var discardWidth = MathF.Max(UiSize.Em(10f), ImGui.CalcTextSize("Discard").X + UiSize.Em(3f));
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ((ImGui.GetContentRegionAvail().X - mainWidth - ImGui.GetStyle().ItemSpacing.X - discardWidth) / 2));

            using (ImRaii.Disabled(candidateCount == 0 || roll.RerollStartedAt >= 0)) {
                if (ImGui.Button(mainLabel, new Vector2(mainWidth, buttonHeight))) {
                    if (roll.CardRevealedAt.Take(roll.Cards.Count).Any(it => it >= 0)) {
                        roll.RerollStartedAt = ImGui.GetTime();
                    } else {
                        roll.Draw(roulette.RouletteCandidates, cardCount);
                    }
                }

                if (ImGui.IsItemHovered()) {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
            }

            if (candidateCount == 0 && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
                ImGui.SetTooltip("No achievement matches these settings");
            }

            ImGui.SameLine();
            if (ImGui.Button("Discard", new Vector2(discardWidth, buttonHeight))) {
                roll.Close();
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
        }
    }

    private static void RollSettings(Plugin plugin, RouletteState roulette, RouletteRoll roll) {
        using var table = ImRaii.Table("RollSettings", 3);
        if (!table) return;

        ImGui.TableSetupColumn("##Categories", ImGuiTableColumnFlags.WidthStretch, 0.5f);
        ImGui.TableSetupColumn("##Gap", ImGuiTableColumnFlags.WidthFixed, UiSize.Em(1f));
        ImGui.TableSetupColumn("##Options", ImGuiTableColumnFlags.WidthStretch, 0.5f);

        ImGui.TableNextColumn();
        RollSettingsCategoryTree(plugin, roulette, roll);

        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        RollSettingsOptions(plugin, roulette);
    }

    private static void RollSettingsCategoryTree(Plugin plugin, RouletteState roulette, RouletteRoll roll) {
        ImGui.TextColored(UiColors.Blue(), $"{roulette.RouletteCandidates.Count:N0}");
        ImGui.SameLine(0, 0);
        ImGui.TextUnformatted(" achievements can be drawn");

        // height is only known once drawn, so the one from the previous frame is used
        using var child = ImRaii.Child("CategoryTree", new Vector2(ImGui.GetContentRegionAvail().X, roll.CategoryTreeHeight), true,
                                       ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        if (!child) return;

        var style = ImGui.GetStyle();
        var disabled = plugin.Configuration.RouletteDisabledCategories;
        var counts = roulette.CategoryCounts;

        foreach (var group in plugin.MainLayout.Achievements) {
            using var id = ImRaii.PushId(group.Name);
            var breadcrumbs = group.Items.Select(it => $"{group.Name} / {it.Name}").ToList();
            var enabledCount = breadcrumbs.Count(it => !disabled.Contains(it));
            var all = enabledCount == breadcrumbs.Count;
            var groupCount = breadcrumbs.Where(it => !disabled.Contains(it)).Sum(it => counts.GetValueOrDefault(it));
            var open = ImGui.TreeNodeEx("##Group", ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowItemOverlap);

            if (ImGui.IsItemHovered()) {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            ImGui.SameLine();

            var checkboxX = ImGui.GetCursorPosX();

            ImGuiP.PushItemFlag(ImGuiItemFlags.MixedValue, enabledCount > 0 && !all);
            if (ImGui.Checkbox(group.Name, ref all)) {
                roulette.UpdateSettings(configuration => {
                    foreach (var breadcrumb in breadcrumbs) {
                        if (all) {
                            configuration.RouletteDisabledCategories.Remove(breadcrumb);
                        } else {
                            configuration.RouletteDisabledCategories.Add(breadcrumb);
                        }
                    }
                });
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            ImGuiP.PopItemFlag();
            UiComponents.SameLineRightTextColored(UiColors.Grey(), $"{groupCount:N0}");

            if (!open) continue;

            var indent = checkboxX + ImGui.GetFrameHeight() + style.ItemInnerSpacing.X - ImGui.GetCursorPosX();

            ImGui.Indent(indent);
            for (var i = 0; i < group.Items.Count; i++) {
                var breadcrumb = breadcrumbs[i];
                var enabled = !disabled.Contains(breadcrumb);
                if (ImGui.Checkbox(group.Items[i].Name, ref enabled)) {
                    roulette.UpdateSettings(configuration => {
                        if (enabled) {
                            configuration.RouletteDisabledCategories.Remove(breadcrumb);
                        } else {
                            configuration.RouletteDisabledCategories.Add(breadcrumb);
                        }
                    });
                }

                if (ImGui.IsItemHovered()) {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }

                UiComponents.SameLineRightTextColored(UiColors.Grey(), $"{counts.GetValueOrDefault(breadcrumb):N0}");
            }

            ImGui.Unindent(indent);
            ImGui.TreePop();
        }

        roll.CategoryTreeHeight = ImGui.GetCursorPosY() - style.ItemSpacing.Y + style.WindowPadding.Y;
    }

    private static void RollSettingsOptions(Plugin plugin, RouletteState roulette) {
        var configuration = plugin.Configuration;
        var spacing = new Vector2(0, UiSize.Em(0.25f) * configuration.UiDensity);
        var available = ImGui.GetContentRegionAvail().X;
        var cardCounts = RouletteRoll.CardCounts;
        var cardCountIndex = Math.Max(0, Array.IndexOf(cardCounts, configuration.RouletteCardCount));
        var cardCount = cardCounts[cardCountIndex];
        var minPointsIndex = Math.Max(0, Array.IndexOf(PointSteps, configuration.RouletteMinPoints));
        var maxPointsIndex = Array.IndexOf(PointSteps, configuration.RouletteMaxPoints) is var index and >= 0 ? index : PointSteps.Length - 1;
        var halfWidth = (available - ImGui.GetStyle().ItemSpacing.X) / 2;
        var nextTierOnly = configuration.RouletteNextTierOnly;
        var onlyWithCounter = configuration.RouletteOnlyWithCounter;
        using var itemWidth = ImRaii.ItemWidth(available);

        ImGui.TextUnformatted("Cards per draw");
        if (ImGui.SliderInt("##CardsPerRoll", ref cardCountIndex, 0, cardCounts.Length - 1, cardCount == 1 ? "1 card" : $"{cardCount} cards")) {
            roulette.UpdateSettings(it => it.RouletteCardCount = cardCounts[cardCountIndex]);
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        ImGui.Dummy(spacing);
        ImGui.TextUnformatted("Points");
        ImGui.SetNextItemWidth(halfWidth);
        if (ImGui.SliderInt("##MinPoints", ref minPointsIndex, 0, PointSteps.Length - 1, $"Min: {PointSteps[minPointsIndex]}")) {
            roulette.UpdateSettings(it => {
                it.RouletteMinPoints = PointSteps[minPointsIndex];
                it.RouletteMaxPoints = PointSteps[Math.Max(maxPointsIndex, minPointsIndex)];
            });
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(halfWidth);
        if (ImGui.SliderInt("##MaxPoints", ref maxPointsIndex, 0, PointSteps.Length - 1, $"Max: {PointSteps[maxPointsIndex]}")) {
            roulette.UpdateSettings(it => {
                it.RouletteMaxPoints = PointSteps[maxPointsIndex];
                it.RouletteMinPoints = PointSteps[Math.Min(minPointsIndex, maxPointsIndex)];
            });
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        ImGui.Dummy(spacing);
        if (ImGui.Checkbox("Next tier of tiered achievements only", ref nextTierOnly)) {
            roulette.UpdateSettings(it => it.RouletteNextTierOnly = nextTierOnly);
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        if (ImGui.Checkbox("Only achievements with a progress counter", ref onlyWithCounter)) {
            roulette.UpdateSettings(it => it.RouletteOnlyWithCounter = onlyWithCounter);
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
    }
}
