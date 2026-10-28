using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Recollection.Data.Unlockable;
using Recollection.Helpers;
using Recollection.UI.Component;
using Recollection.UI.State;

namespace Recollection.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    private const string NoDataText = "No data.";
    private const string Ellipsis = "...";

    private static readonly Dictionary<uint, (float MaxWidth, string Text)> TruncatedNames = new();

    private readonly record struct ActivityEntry(UnlockableAchievement Achievement, string? Breadcrumb, string? Detail);

    private static string FormatPercentage(double ratio) => $"{(int)MathF.Round((float)ratio * 100)}%";

    public static void ActivityColumns(Plugin plugin, UnlockablesState unlockables) {
        if (!ImGui.BeginTable("ActivityColumns", 3)) return;

        ImGui.TableSetupColumn("##Left", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("##MiddleGap", ImGuiTableColumnFlags.WidthFixed, UiSize.Em(1f));
        ImGui.TableSetupColumn("##Right", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextColumn();
        RecentlyObtainedColumn(plugin, unlockables);

        ImGui.TableNextColumn();

        ImGui.TableNextColumn();
        NearingCompletionColumn(plugin, unlockables);

        ImGui.EndTable();
    }

    private static void RecentlyObtainedColumn(Plugin plugin, UnlockablesState unlockables) {
        var entries = unlockables.RecentlyUnlockedAchievements.Select(update => {
            var achievement = plugin.UnlockablesService.GetUnlockableAchievement(update.AchievementId);
            var breadcrumb = unlockables.FindBreadcrumb(achievement.Id);
            return new ActivityEntry(achievement, breadcrumb, TimeFormat.FormatTimeAgo(update.Timestamp));
        }).ToList();

        ActivityColumn(plugin, "Recently Obtained", entries);
    }

    private static void NearingCompletionColumn(Plugin plugin, UnlockablesState unlockables) {
        var entries = unlockables.NearingCompletionAchievements.Select(candidate => {
            var breadcrumb = unlockables.FindBreadcrumb(candidate.Achievement.Id);
            return new ActivityEntry(candidate.Achievement, breadcrumb, FormatPercentage(candidate.Ratio));
        }).ToList();

        ActivityColumn(plugin, "Nearing Completion", entries);
    }

    private static void ActivityColumn(Plugin plugin, string title, IReadOnlyList<ActivityEntry> entries) {
        UiComponents.SectionHeader(title, plugin.Configuration.UiDensity, UiFonts.FontSize110, paddingAboveEm: 0f);

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

    private static void ActivityRow(ActivityEntry entry) {
        var achievement = entry.Achievement;
        var iconSize = ImGui.GetTextLineHeight();
        var pointsText = $"+{achievement.Points}";
        var rightWidth = ImGui.CalcTextSize(pointsText).X;
        if (entry.Detail != null) {
            rightWidth += ImGui.CalcTextSize(entry.Detail).X + UiSize.Em(0.5f);
        }
        var maxNameWidth = ImGui.GetContentRegionAvail().X - iconSize - ImGui.GetStyle().ItemSpacing.X - rightWidth - UiSize.Em(0.5f);

        ImGui.BeginGroup();

        var wrap = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(achievement.Icon)).GetWrapOrEmpty();
        ImGui.Image(wrap.Handle, new Vector2(iconSize, iconSize));

        ImGui.SameLine();
        ImGui.TextColored(UiColors.Text(), TruncateName(achievement, maxNameWidth));

        if (entry.Detail != null) {
            RightAlignedTwoPart(entry.Detail, UiColors.Grey(), pointsText, UiColors.Progress());
        } else {
            UiComponents.SameLineRightTextColored(UiColors.Progress(), pointsText);
        }

        ImGui.EndGroup();

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(BuildTooltip(entry));
    }

    private static string TruncateName(UnlockableAchievement achievement, float maxWidth) {
        if (TruncatedNames.TryGetValue(achievement.Id, out var cached) && Math.Abs(cached.MaxWidth - maxWidth) < 5f) return cached.Text;

        var text = achievement.Name;
        if (ImGui.CalcTextSize(text).X > maxWidth) {
            var available = maxWidth - ImGui.CalcTextSize(Ellipsis).X;
            var low = 0;
            var high = text.Length;
            while (low < high) {
                var mid = (low + high + 1) / 2;
                if (ImGui.CalcTextSize(text[..mid]).X <= available) {
                    low = mid;
                } else {
                    high = mid - 1;
                }
            }
            text = text[..low].TrimEnd() + Ellipsis;
        }

        TruncatedNames[achievement.Id] = (maxWidth, text);
        return text;
    }

    private static string BuildTooltip(ActivityEntry entry) => $"In {entry.Breadcrumb ?? "(unknown)"}\n{entry.Achievement.Description}";

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
