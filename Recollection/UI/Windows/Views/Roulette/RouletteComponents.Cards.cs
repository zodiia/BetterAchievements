using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Recollection.Helpers;
using Recollection.UI.Component;
using Recollection.UI.State;

namespace Recollection.UI.Windows.Views.Roulette;

public static partial class RouletteComponents {
    private const float CardRevealDuration = 0.5f;
    private const float CardBorderThickness = 1.5f;
    private const float CardPaddingXEm = 1f;
    private const float CardPaddingYEm = 1f;
    private const float CardQuestionMarkSpacingEm = 2f;

    private static void Cards(RouletteState roulette, RouletteRoll roll) {
        var cards = roll.Cards;
        var style = ImGui.GetStyle();
        var startX = ImGui.GetCursorPosX();
        var cardWidth = (ImGui.GetContentRegionAvail().X - style.ItemSpacing.X) / 2;
        var contentWidth = cardWidth - (UiSize.Em(CardPaddingXEm) * 2);

        for (var i = 0; i < cards.Count; i++) {
            var pairEnd = Math.Min(i + (i % 2 == 0 ? 2 : 1), cards.Count);
            var pairStart = i - (i % 2);
            var x = pairEnd - pairStart == 1 ? startX + ((cardWidth + style.ItemSpacing.X) / 2) : startX + ((i % 2) * (cardWidth + style.ItemSpacing.X));
            var contentHeight = 0f;

            for (var j = pairStart; j < pairEnd; j++) {
                contentHeight = MathF.Max(contentHeight, CardContentHeight(cards[j], contentWidth));
            }

            var cardHeight = UiSize.Em(CardPaddingYEm) + contentHeight + UiSize.Em(0.5f) + ImGui.GetFrameHeight() + UiSize.Em(CardPaddingYEm);

            if (i % 2 == 1) {
                ImGui.SameLine();
            }

            Card(roulette, roll, cards[i], i, x, cardWidth, cardHeight);
            if (cards.Count == 0) {
                return;
            }
        }
    }

    private static void Card(RouletteState roulette, RouletteRoll roll, AchievementPoolEntry entry, int index, float startX, float width, float height) {
        var now = ImGui.GetTime();
        var revealedAt = roll.CardRevealedAt[index];
        var reveal = revealedAt < 0 ? 0f : Math.Clamp((float)(now - revealedAt) / CardRevealDuration, 0f, 1f);
        var halfThickness = new Vector2(CardBorderThickness / 2);

        if (roll.RerollStartedAt >= 0) {
            reveal = MathF.Min(reveal, 1f - Math.Clamp((float)(now - roll.RerollStartedAt) / CardRevealDuration, 0f, 1f));
        }

        var visibleWidth = MathF.Max(1f, width * MathF.Abs(MathF.Cos(reveal * MathF.PI)));
        ImGui.SetCursorPosX(startX + ((width - visibleWidth) / 2));

        using var child = ImRaii.Child($"Card{index}", new Vector2(visibleWidth, height), false,
                                       ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        if (!child) return;

        var min = ImGui.GetWindowPos();
        var max = min + ImGui.GetWindowSize();

        if (reveal < 0.5f) {
            if (CardBack(revealedAt < 0 && roll.RerollStartedAt < 0)) {
                roll.CardRevealedAt[index] = now;
            }
        } else {
            ImGui.SetCursorPos(new Vector2(((visibleWidth - width) / 2) + UiSize.Em(CardPaddingXEm), UiSize.Em(CardPaddingYEm)));
            CardContent(roulette, roll, entry, width - (UiSize.Em(CardPaddingXEm) * 2), reveal);
        }

        if (roll.Cards.Count == 0) return;

        ImGui.GetWindowDrawList().AddRect(min + halfThickness, max - halfThickness, ImGui.GetColorU32(UiColors.Progress() with { W = 0.5f }),
                                          ImGui.GetStyle().FrameRounding, ImDrawFlags.None, CardBorderThickness);
    }

    private static float CardContentHeight(AchievementPoolEntry entry, float contentWidth) {
        var achievement = ExcelSheets.Achievement.Value.GetRow(entry.Id);
        var spacing = ImGui.GetStyle().ItemSpacing.Y;
        float nameHeight;
        using (UiFonts.FontSize110().Push()) nameHeight = ImGui.CalcTextSize(achievement.Name.ToString(), false, contentWidth).Y;

        return UiSize.Em(3.5f) + spacing +
               nameHeight + spacing +
               ImGui.GetTextLineHeight() + spacing +
               ImGui.CalcTextSize(entry.Breadcrumb, false, contentWidth).Y + spacing +
               1f + spacing +
               ImGui.CalcTextSize(achievement.Description.ToString(), false, contentWidth).Y + spacing;
    }

    private static void CardContent(RouletteState roulette, RouletteRoll roll, AchievementPoolEntry entry, float contentWidth, float reveal) {
        var achievement = ExcelSheets.Achievement.Value.GetRow(entry.Id);
        var startX = ImGui.GetCursorPosX();
        var iconSize = UiSize.Em(3.5f);
        var wrap = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(achievement.Icon)).GetWrapOrEmpty();
        var name = achievement.Name.ToString();
        var points = $"{achievement.Points} points";

        ImGui.Indent(startX);
        ImGui.PushTextWrapPos(startX + contentWidth);

        ImGui.SetCursorPosX(startX + ((contentWidth - iconSize) / 2));
        ImGui.Image(wrap.Handle, new Vector2(iconSize, iconSize));

        using (UiFonts.FontSize110().Push()) {
            var nameWidth = ImGui.CalcTextSize(name).X;
            if (nameWidth <= contentWidth) {
                ImGui.SetCursorPosX(startX + ((contentWidth - nameWidth) / 2));
            }

            using (ImRaii.PushColor(ImGuiCol.Text, UiColors.Progress())) ImGui.TextWrapped(name);
        }

        ImGui.SetCursorPosX(startX + ((contentWidth - ImGui.CalcTextSize(points).X) / 2));
        ImGui.TextColored(UiColors.Progress(), points);

        using (ImRaii.PushColor(ImGuiCol.Text, UiColors.Grey())) ImGui.TextWrapped(entry.Breadcrumb);

        var lineStart = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddLine(lineStart, lineStart with { X = lineStart.X + contentWidth }, ImGui.GetColorU32(ImGuiCol.Separator));
        ImGui.Dummy(new Vector2(contentWidth, 1f));

        ImGui.TextWrapped(achievement.Description.ToString());
        ImGui.PopTextWrapPos();

        ImGui.SetCursorPosY(ImGui.GetWindowSize().Y - ImGui.GetFrameHeight() - UiSize.Em(CardPaddingYEm));
        using (ImRaii.Disabled(reveal < 1f)) {
            if (ImGui.Button(roll.Cards.Count == 1 ? "Accept" : "Pick this one", new Vector2(contentWidth, 0))) {
                roulette.Roll(entry.Id);
                roll.Close();
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
        }

        ImGui.Unindent(startX);
    }

    private static bool CardBack(bool clickable) {
        var style = ImGui.GetStyle();
        var drawList = ImGui.GetWindowDrawList();
        var min = ImGui.GetWindowPos();
        var max = min + ImGui.GetWindowSize();

        ImGui.SetCursorPos(Vector2.Zero);
        var clicked = ImGui.InvisibleButton("##Back", ImGui.GetWindowSize());
        var hovered = clickable && ImGui.IsItemHovered();

        drawList.AddRectFilled(min, max, ImGui.GetColorU32(UiColors.WindowBackground() with { W = 1f }), style.FrameRounding);
        drawList.AddRectFilled(min, max, ImGui.GetColorU32(UiColors.Progress() with { W = 0.04f }), style.FrameRounding);

        TiltedQuestionMarks(min + new Vector2(CardBorderThickness), max - new Vector2(CardBorderThickness),
                            UiColors.Progress() with { W = hovered ? 0.25f : 0.15f });

        if (!hovered) return false;

        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        ImGui.SetTooltip("Click to reveal");
        return clicked;
    }

    private static void TiltedQuestionMarks(Vector2 boundStart, Vector2 boundEnd, Vector4 color) {
        var drawList = ImGui.GetWindowDrawList();
        var center = (boundStart + boundEnd) / 2;
        var fontSize = ImGui.GetFontSize() * 1.6f;
        var textSize = ImGui.CalcTextSize("?") * 1.6f;
        var spacing = UiSize.Em(CardQuestionMarkSpacingEm);
        var colorU32 = ImGui.GetColorU32(color);
        var (sin, cos) = MathF.SinCos(-0.35f);
        var row = 0;

        drawList.PushClipRect(boundStart, boundEnd, true);
        for (var y = center.Y - (MathF.Ceiling((center.Y - boundStart.Y) / spacing) * spacing); y < boundEnd.Y + spacing; y += spacing, row++) {
            var rowOffset = (row % 2) * spacing / 2;
            for (var x = center.X - (MathF.Ceiling((center.X - boundStart.X) / spacing) * spacing) - rowOffset; x < boundEnd.X + spacing; x += spacing) {
                var position = new Vector2(x, y);
                var vertexStart = drawList.VtxBuffer.Size;
                drawList.AddText(ImGui.GetFont(), fontSize, position - (textSize / 2), colorU32, "?");

                for (var i = vertexStart; i < drawList.VtxBuffer.Size; i++) {
                    ref var vertex = ref drawList.VtxBuffer.Ref<ImDrawVert>(i);
                    var offset = vertex.Pos - position;
                    vertex.Pos = position + new Vector2((offset.X * cos) - (offset.Y * sin), (offset.X * sin) + (offset.Y * cos));
                }
            }
        }

        drawList.PopClipRect();
    }
}
