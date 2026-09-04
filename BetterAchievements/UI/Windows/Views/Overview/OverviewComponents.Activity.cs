using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.Services;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;

namespace BetterAchievements.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    private const int ActivityRowCount = 10;

    private static MainLayout? SampleIdsSource;
    private static List<uint> SampleIdsCache = [];

    private static List<uint> SampleAchievementIds(UnlockablesState unlockables) {
        if (!ReferenceEquals(SampleIdsSource, unlockables.FilteredLayout)) {
            SampleIdsSource = unlockables.FilteredLayout;
            SampleIdsCache = unlockables.FilteredLayout.AchievementLayout
                                        .SelectMany(it => it.GetAllAchievementIds())
                                        .Take(ActivityRowCount)
                                        .ToList();
        }

        return SampleIdsCache;
    }

    public static void ActivityColumns(Plugin plugin, UnlockablesState unlockables) {
        if (!ImGui.BeginTable("ActivityColumns", 3)) return;

        ImGui.TableSetupColumn("##Left", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("##MiddleGap", ImGuiTableColumnFlags.WidthFixed, UiSize.Em(1f));
        ImGui.TableSetupColumn("##Right", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextColumn();
        RecentlyObtainedColumn(plugin, unlockables.RecentlyUnlockedAchievements);

        ImGui.TableNextColumn();

        ImGui.TableNextColumn();
        ActivityColumn(plugin, "Nearing Completion", SampleAchievementIds(unlockables), null);

        ImGui.EndTable();
    }

    private static void RecentlyObtainedColumn(Plugin plugin, IReadOnlyList<AchievementUpdate> updates) {
        UiComponents.SeparatorText("Recently Obtained", UiFonts.FontSize110, paddingAboveEm: 0f);

        foreach (var update in updates) {
            var achievement = plugin.UnlockablesService.GetUnlockableAchievement(update.AchievementId);
            ActivityRow(achievement.Icon(), achievement.Name(), achievement.Points(), FormatTimeAgo(update.Timestamp));

            // yes it is on purpose, for some reason an empty dummy still adds spacing?
            // me from the future: yes you idiot it's the item padding
            ImGui.Dummy(new());
        }
    }

    private static void ActivityColumn(Plugin plugin, string title, IEnumerable<uint> achievementIds, string[]? dummyDetails) {
        UiComponents.SeparatorText(title, UiFonts.FontSize110, paddingAboveEm: 0f);

        var index = 0;
        foreach (var id in achievementIds) {
            var achievement = plugin.UnlockablesService.GetUnlockableAchievement(id);
            var detail = dummyDetails?[index % dummyDetails.Length];
            ActivityRow(achievement.Icon(), achievement.Name(), achievement.Points(), detail);
            index++;

            // yes it is on purpose, for some reason an empty dummy still adds spacing?
            // me from the future: yes you idiot it's the item padding
            ImGui.Dummy(new());
        }
    }

    private static string FormatTimeAgo(ulong timestamp) {
        var elapsed = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds((long)timestamp);

        return elapsed switch {
            { TotalMinutes: < 1 } => "just now",
            { TotalHours: < 1 } => FormatUnit((int)elapsed.TotalMinutes, "minute"),
            { TotalDays: < 1 } => FormatUnit((int)elapsed.TotalHours, "hour"),
            { TotalDays: < 7 } => FormatUnit((int)elapsed.TotalDays, "day"),
            { TotalDays: < 30 } => FormatUnit((int)(elapsed.TotalDays / 7), "week"),
            { TotalDays: < 365 } => FormatUnit((int)(elapsed.TotalDays / 30), "month"),
            _ => FormatUnit((int)(elapsed.TotalDays / 365), "year"),
        };
    }

    private static string FormatUnit(int amount, string unit) => $"{amount} {unit}{(amount == 1 ? "" : "s")} ago";

    private static void ActivityRow(uint iconId, string name, byte points, string? detail) {
        var iconSize = ImGui.GetTextLineHeight();

        var wrap = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrEmpty();
        ImGui.Image(wrap.Handle, new Vector2(iconSize, iconSize));

        ImGui.SameLine();
        ImGui.TextColored(UiColors.Text(), name);

        var pointsText = $"+{points}";
        if (detail != null) {
            RightAlignedTwoPart(detail, UiColors.Grey(), pointsText, UiColors.Progress());
        } else {
            UiComponents.SameLineRightTextColored(UiColors.Progress(), pointsText);
        }
    }

    private static void RightAlignedTwoPart(string leftText, Vector4 leftColor, string rightText, Vector4 rightColor) {
        var spacing = ImGui.CalcTextSize(" ").X;
        var totalWidth = ImGui.CalcTextSize(leftText).X + spacing + ImGui.CalcTextSize(rightText).X;
        var targetX = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - totalWidth;

        ImGui.SameLine();
        ImGui.SetCursorPosX(targetX);
        ImGui.TextColored(leftColor, leftText);
        ImGui.SameLine(0, spacing);
        ImGui.TextColored(rightColor, rightText);
    }
}
