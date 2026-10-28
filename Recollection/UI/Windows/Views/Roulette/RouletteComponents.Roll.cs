using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Recollection.Data;
using Recollection.UI.Component;
using Recollection.UI.State;

namespace Recollection.UI.Windows.Views.Roulette;

public static partial class RouletteComponents {
    private static readonly int[] PointSteps = [0, 5, 10, 20, 30, 50];

    private static float RollButtonHeight => UiSize.Em(2.5f);
    private static float CheckboxWidth => ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X;

    private static void HandCursorOnHover() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
    }

    public static void RollSection(Plugin plugin, RouletteState roulette, RouletteRoll roll) {
        if (!roll.IsOpen) {
            OpenRollButton(plugin, roll);
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

        RollActions(plugin, roulette, roll);
    }

    private static void OpenRollButton(Plugin plugin, RouletteRoll roll) {
        ImGui.Dummy(new Vector2(0, UiSize.Em(0.75f) * plugin.Configuration.UiDensity));
        using (UiFonts.FontSize125().Push()) {
            if (ImGui.Button("Roll new achievement", new Vector2(ImGui.GetContentRegionAvail().X, RollButtonHeight))) {
                roll.IsOpen = true;
            }

            HandCursorOnHover();
        }
    }

    private static void RollActions(Plugin plugin, RouletteState roulette, RouletteRoll roll) {
        using var font = UiFonts.FontSize125().Push();
        var cardCount = plugin.Configuration.RouletteCardCount;
        var mainLabel = roll.Cards.Count > 0 ? "Reroll" : cardCount == 1 ? "Draw a card" : $"Draw {cardCount} cards";
        var mainWidth = MathF.Max(UiSize.Em(10f), ImGui.CalcTextSize(mainLabel).X + UiSize.Em(3f));
        var discardWidth = MathF.Max(UiSize.Em(10f), ImGui.CalcTextSize("Discard").X + UiSize.Em(3f));

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ((ImGui.GetContentRegionAvail().X - mainWidth - ImGui.GetStyle().ItemSpacing.X - discardWidth) / 2));
        DrawButton(roulette, roll, mainLabel, mainWidth, cardCount);

        ImGui.SameLine();
        if (ImGui.Button("Discard", new Vector2(discardWidth, RollButtonHeight))) {
            roll.Close();
        }

        HandCursorOnHover();
    }

    private static void DrawButton(RouletteState roulette, RouletteRoll roll, string label, float width, int cardCount) {
        var candidateCount = roulette.RouletteCandidates.Count;

        using (ImRaii.Disabled(candidateCount == 0 || roll.RerollStartedAt >= 0)) {
            if (ImGui.Button(label, new Vector2(width, RollButtonHeight))) {
                if (roll.CardRevealedAt.Take(roll.Cards.Count).Any(it => it >= 0)) {
                    roll.RerollStartedAt = ImGui.GetTime();
                } else {
                    roll.Draw(roulette.RouletteCandidates, cardCount);
                }
            }

            HandCursorOnHover();
        }

        if (candidateCount == 0 && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
            ImGui.SetTooltip("No achievement matches these settings");
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
        var style = ImGui.GetStyle();

        ImGui.TextColored(UiColors.Blue(), $"{roulette.RouletteCandidates.Count:N0}");
        ImGui.SameLine(0, 0);
        ImGui.TextUnformatted(" achievements can be drawn");

        // height is only known once drawn, so the one from the previous frame is used
        using var child = ImRaii.Child("CategoryTree", new Vector2(ImGui.GetContentRegionAvail().X, roll.CategoryTreeHeight), true,
                                       ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        if (!child) return;

        CategoryTreeSelectButtons(roulette);
        foreach (var group in plugin.MainLayout.Achievements) {
            CategoryTreeGroup(plugin, roulette, group);
        }

        roll.CategoryTreeHeight = ImGui.GetCursorPosY() - style.ItemSpacing.Y + style.WindowPadding.Y;
    }

    private static void CategoryTreeSelectButtons(RouletteState roulette) {
        if (ImGui.SmallButton("Select all")) {
            roulette.UpdateSettings(configuration => SetSectionsEnabled(configuration, roulette.PoolSections.ToList(), true));
        }

        HandCursorOnHover();
        ImGui.SameLine();
        if (ImGui.SmallButton("Select none")) {
            roulette.UpdateSettings(configuration => SetSectionsEnabled(configuration, roulette.PoolSections.ToList(), false));
        }

        HandCursorOnHover();
    }

    private static void CategoryTreeGroup(Plugin plugin, RouletteState roulette, AchievementLayoutGroup group) {
        var sections = group.Items.SelectMany(category => SectionKeys(group, category)).Where(roulette.PoolSections.Contains).ToList();
        if (sections.Count == 0) return;

        // categories are tree nodes too, so their checkbox is shifted by the checkbox width only, minus the indent the tree node already pushes
        var indent = CheckboxWidth - ImGui.GetStyle().IndentSpacing;

        using var id = ImRaii.PushId(group.Name);
        if (!CategoryTreeNode(plugin, roulette, group.Name, sections, out _)) return;

        ImGui.Indent(indent);
        foreach (var category in group.Items) {
            CategoryTreeCategory(plugin, roulette, group, category);
        }

        ImGui.Unindent(indent);
        ImGui.TreePop();
    }

    private static void CategoryTreeCategory(Plugin plugin, RouletteState roulette, AchievementLayoutGroup group, AchievementLayoutCategory category) {
        var sections = SectionKeys(group, category).Where(roulette.PoolSections.Contains).ToList();
        if (sections.Count == 0) return;

        using var id = ImRaii.PushId(category.Name);
        if (!CategoryTreeNode(plugin, roulette, category.Name, sections, out var checkboxIndent)) return;

        var indent = checkboxIndent + CheckboxWidth;

        ImGui.Indent(indent);
        foreach (var section in sections) {
            CategoryTreeSection(plugin, roulette, section);
        }

        ImGui.Unindent(indent);
        ImGui.TreePop();
    }

    private static void CategoryTreeSection(Plugin plugin, RouletteState roulette, string section) {
        var enabled = !plugin.Configuration.RouletteDisabledCategories.Contains(section);
        var label = section[(section.LastIndexOf(" / ", StringComparison.Ordinal) + 3)..];

        if (ImGui.Checkbox(label, ref enabled)) {
            roulette.UpdateSettings(configuration => SetSectionsEnabled(configuration, [section], enabled));
        }

        HandCursorOnHover();
        UiComponents.SameLineRightTextColored(UiColors.Grey(), $"{roulette.SectionCounts.GetValueOrDefault(section):N0}");
    }

    private static IEnumerable<string> SectionKeys(AchievementLayoutGroup group, AchievementLayoutCategory category) =>
        category.Sections.Select(section => $"{group.Name} / {category.Name} / {section.Name}");

    private static void SetSectionsEnabled(Configuration configuration, List<string> sections, bool enabled) {
        foreach (var section in sections) {
            if (enabled) {
                configuration.RouletteDisabledCategories.Remove(section);
            } else {
                configuration.RouletteDisabledCategories.Add(section);
            }
        }
    }

    private static bool CategoryTreeNode(Plugin plugin, RouletteState roulette, string label, List<string> sections, out float checkboxIndent) {
        var disabled = plugin.Configuration.RouletteDisabledCategories;
        var enabledCount = sections.Count(it => !disabled.Contains(it));
        var all = enabledCount == sections.Count;
        var count = sections.Where(it => !disabled.Contains(it)).Sum(it => roulette.SectionCounts.GetValueOrDefault(it));
        var open = ImGui.TreeNodeEx("##Node", ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowItemOverlap);

        HandCursorOnHover();
        ImGui.SameLine();

        var checkboxX = ImGui.GetCursorPosX();

        ImGuiP.PushItemFlag(ImGuiItemFlags.MixedValue, enabledCount > 0 && !all);
        if (ImGui.Checkbox(label, ref all)) {
            roulette.UpdateSettings(configuration => SetSectionsEnabled(configuration, sections, all));
        }

        HandCursorOnHover();
        ImGuiP.PopItemFlag();
        UiComponents.SameLineRightTextColored(UiColors.Grey(), $"{count:N0}");

        checkboxIndent = checkboxX - ImGui.GetCursorPosX();
        return open;
    }

    private static void RollSettingsOptions(Plugin plugin, RouletteState roulette) {
        var configuration = plugin.Configuration;
        var spacing = new Vector2(0, UiSize.Em(0.25f) * configuration.UiDensity);
        using var itemWidth = ImRaii.ItemWidth(ImGui.GetContentRegionAvail().X);

        CardCountSlider(configuration, roulette);
        ImGui.Dummy(spacing);
        PointsSliders(configuration, roulette);
        ImGui.Dummy(spacing);
        OptionCheckbox(roulette, "Next tier of tiered achievements only", configuration.RouletteNextTierOnly,
                       (it, value) => it.RouletteNextTierOnly = value);
        OptionCheckbox(roulette, "Only achievements with a progress counter", configuration.RouletteOnlyWithCounter,
                       (it, value) => it.RouletteOnlyWithCounter = value);
        OptionCheckbox(roulette, "Only achievements available based on current story progression", configuration.RouletteOnlyAvailable,
                       (it, value) => it.RouletteOnlyAvailable = value);

        using (ImRaii.Disabled()) {
            OptionCheckbox(roulette, "Only achievements that have an item or title reward", configuration.RouletteOnlyWithRewards,
                           (it, value) => it.RouletteOnlyWithRewards = value);
        }
    }

    private static void OptionCheckbox(RouletteState roulette, string label, bool value, Action<Configuration, bool> update) {
        if (ImGui.Checkbox(label, ref value)) {
            roulette.UpdateSettings(it => update(it, value));
        }

        HandCursorOnHover();
    }

    private static void CardCountSlider(Configuration configuration, RouletteState roulette) {
        var cardCounts = RouletteRoll.CardCounts;
        var cardCountIndex = Math.Max(0, Array.IndexOf(cardCounts, configuration.RouletteCardCount));
        var cardCount = cardCounts[cardCountIndex];

        ImGui.TextUnformatted("Cards per draw");
        if (ImGui.SliderInt("##CardsPerRoll", ref cardCountIndex, 0, cardCounts.Length - 1, cardCount == 1 ? "1 card" : $"{cardCount} cards")) {
            roulette.UpdateSettings(it => it.RouletteCardCount = cardCounts[cardCountIndex]);
        }

        HandCursorOnHover();
    }

    private static void PointsSliders(Configuration configuration, RouletteState roulette) {
        var minPointsIndex = Math.Max(0, Array.IndexOf(PointSteps, configuration.RouletteMinPoints));
        var maxPointsIndex = Array.IndexOf(PointSteps, configuration.RouletteMaxPoints) is var index and >= 0 ? index : PointSteps.Length - 1;
        var halfWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;

        ImGui.TextUnformatted("Points");
        ImGui.SetNextItemWidth(halfWidth);
        if (ImGui.SliderInt("##MinPoints", ref minPointsIndex, 0, PointSteps.Length - 1, $"Min: {PointSteps[minPointsIndex]}")) {
            roulette.UpdateSettings(it => {
                it.RouletteMinPoints = PointSteps[minPointsIndex];
                it.RouletteMaxPoints = PointSteps[Math.Max(maxPointsIndex, minPointsIndex)];
            });
        }

        HandCursorOnHover();
        ImGui.SameLine();
        ImGui.SetNextItemWidth(halfWidth);
        if (ImGui.SliderInt("##MaxPoints", ref maxPointsIndex, 0, PointSteps.Length - 1, $"Max: {PointSteps[maxPointsIndex]}")) {
            roulette.UpdateSettings(it => {
                it.RouletteMaxPoints = PointSteps[maxPointsIndex];
                it.RouletteMinPoints = PointSteps[Math.Min(minPointsIndex, maxPointsIndex)];
            });
        }

        HandCursorOnHover();
    }
}
