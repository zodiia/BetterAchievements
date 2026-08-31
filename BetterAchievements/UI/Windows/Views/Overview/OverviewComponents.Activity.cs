using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BetterAchievements.Data;
using BetterAchievements.UI.Component;
using BetterAchievements.UI.State;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;

namespace BetterAchievements.UI.Windows.Views.Overview;

public static partial class OverviewComponents {
    private const int ActivityRowCount = 10;

    private static readonly string[] RecentlyObtainedDummyDetails = [
        "2 hours ago", "1 day ago", "3 days ago", "5 days ago", "1 week ago",
        "2 weeks ago", "3 weeks ago", "1 month ago", "2 months ago", "3 months ago"
    ];

    private static MainLayout? SampleIdsSource;
    private static List<uint> SampleIdsCache = [];

    private static List<uint> SampleAchievementIds(UnlockablesState unlockables) {
        if (!ReferenceEquals(SampleIdsSource, unlockables.FilteredLayout)) {
            SampleIdsSource = unlockables.FilteredLayout;
            SampleIdsCache = unlockables.FilteredLayout.AchievementLayout
                                        .SelectMany(it => it.GetAllAchievementIds())
                                        .Take(ActivityRowCount * 2)
                                        .ToList();
        }

        return SampleIdsCache;
    }

    public static void ActivityColumns(Plugin plugin, UnlockablesState unlockables) {
        if (!ImGui.BeginTable("ActivityColumns", 3)) return;

        ImGui.TableSetupColumn("##Left", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("##MiddleGap", ImGuiTableColumnFlags.WidthFixed, UiSize.Em(1f));
        ImGui.TableSetupColumn("##Right", ImGuiTableColumnFlags.WidthStretch);

        var sampleIds = SampleAchievementIds(unlockables);

        ImGui.TableNextColumn();
        ActivityColumn(plugin, "Recently Obtained", sampleIds.Take(ActivityRowCount), RecentlyObtainedDummyDetails);

        ImGui.TableNextColumn();

        ImGui.TableNextColumn();
        ActivityColumn(plugin, "Nearing Completion", sampleIds.Skip(ActivityRowCount).Take(ActivityRowCount), null);

        ImGui.EndTable();
    }

    private static void ActivityColumn(Plugin plugin, string title, IEnumerable<uint> achievementIds, string[]? dummyDetails) {
        UiComponents.SeparatorText(title, UiFonts.FontSize110, paddingAboveEm: 0f);

        var index = 0;
        foreach (var id in achievementIds) {
            var achievement = plugin.UnlockablesService.GetUnlockableAchievement(id);
            var detail = dummyDetails?[index % dummyDetails.Length];
            ActivityRow(achievement.Icon(), achievement.Name(), achievement.Points(), detail);
            index++;

            ImGui.Dummy(new()); // yes it is on purpose, for some reason an empty dummy still adds spacing?
        }
    }

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
