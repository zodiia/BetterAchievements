using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.Data.Unlockable;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;

namespace BetterAchievements.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    private const int ActivityRowCount = 10;
    private const string NoDataText = "No data.";

    private readonly record struct ActivityEntry(UnlockableAchievement Achievement, string? Breadcrumb, string? Detail);
    private readonly record struct RankedEntry(double Ratio, ActivityEntry Entry);

    private static MainLayout? NearingCompletionSource;
    private static List<ActivityEntry> NearingCompletionCache = [];

    private static IEnumerable<AchievementLayoutItem> AllItems(AchievementLayout layout) => layout switch {
        AchievementLayoutGroup group => group.Items.SelectMany(AllItems),
        AchievementLayoutCategory category => category.Items,
        _ => []
    };

    private static string FormatPercentage(double ratio) => $"{(int)MathF.Round((float)ratio * 100)}%";

    private static RankedEntry? NearingCompletionCandidate(Plugin plugin, UnlockablesState unlockables, AchievementLayoutItem item) {
        var candidate = item switch {
            AchievementLayoutItemSimple simple => plugin.UnlockablesService.GetUnlockableAchievement(simple.Id).NearingCompletionCandidate(),
            AchievementLayoutItemTiered tiered => plugin.UnlockablesService.GetUnlockableTieredAchievement(tiered.Ids, tiered.Spoilers).NearingCompletionCandidate(),
            _ => null
        };

        if (candidate == null) return null;

        var breadcrumb = unlockables.FindBreadcrumb(candidate.Achievement.Id());
        return new RankedEntry(candidate.Ratio, new ActivityEntry(candidate.Achievement, breadcrumb, FormatPercentage(candidate.Ratio)));
    }

    private static List<ActivityEntry> NearingCompletionAchievements(Plugin plugin, UnlockablesState unlockables) {
        if (ReferenceEquals(NearingCompletionSource, unlockables.FilteredLayout)) return NearingCompletionCache;
        NearingCompletionSource = unlockables.FilteredLayout;

        NearingCompletionCache = plugin.MainLayout.AchievementLayout
                                        .SelectMany(AllItems)
                                        .Select(item => NearingCompletionCandidate(plugin, unlockables, item))
                                        .Where(it => it != null)
                                        .Select(it => it!.Value)
                                        .OrderByDescending(it => it.Ratio)
                                        .Take(ActivityRowCount)
                                        .Select(it => it.Entry)
                                        .ToList();

        return NearingCompletionCache;
    }

    public static void ActivityColumns(Plugin plugin, UnlockablesState unlockables) {
        if (!ImGui.BeginTable("ActivityColumns", 3)) return;

        ImGui.TableSetupColumn("##Left", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("##MiddleGap", ImGuiTableColumnFlags.WidthFixed, UiSize.Em(1f));
        ImGui.TableSetupColumn("##Right", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextColumn();
        RecentlyObtainedColumn(plugin, unlockables);

        ImGui.TableNextColumn();

        ImGui.TableNextColumn();
        ActivityColumn("Nearing Completion", NearingCompletionAchievements(plugin, unlockables));

        ImGui.EndTable();
    }

    private static void RecentlyObtainedColumn(Plugin plugin, UnlockablesState unlockables) {
        var entries = unlockables.RecentlyUnlockedAchievements.Select(update => {
            var achievement = plugin.UnlockablesService.GetUnlockableAchievement(update.AchievementId);
            var breadcrumb = unlockables.FindBreadcrumb(achievement.Id());
            return new ActivityEntry(achievement, breadcrumb, FormatTimeAgo(update.Timestamp));
        }).ToList();

        ActivityColumn("Recently Obtained", entries);
    }

    private static void ActivityColumn(string title, IReadOnlyList<ActivityEntry> entries) {
        UiComponents.SeparatorText(title, UiFonts.FontSize110, paddingAboveEm: 0f);

        if (entries.Count == 0) {
            ImGui.TextColored(UiColors.Grey(), NoDataText);
            return;
        }

        foreach (var entry in entries) {
            ActivityRow(entry);

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

    private static void ActivityRow(ActivityEntry entry) {
        var achievement = entry.Achievement;
        var iconSize = ImGui.GetTextLineHeight();

        ImGui.BeginGroup();

        var wrap = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(achievement.Icon())).GetWrapOrEmpty();
        ImGui.Image(wrap.Handle, new Vector2(iconSize, iconSize));

        ImGui.SameLine();
        ImGui.TextColored(UiColors.Text(), achievement.Name());

        var pointsText = $"+{achievement.Points()}";
        if (entry.Detail != null) {
            RightAlignedTwoPart(entry.Detail, UiColors.Grey(), pointsText, UiColors.Progress());
        } else {
            UiComponents.SameLineRightTextColored(UiColors.Progress(), pointsText);
        }

        ImGui.EndGroup();

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(BuildTooltip(entry));
    }

    private static string BuildTooltip(ActivityEntry entry) => $"In {entry.Breadcrumb ?? "(unknown)"}\n{entry.Achievement.Description()}";

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
