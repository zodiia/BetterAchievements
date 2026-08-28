using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace BetterAchievements.UI.Component;

// re-implementation of ImGui.Clipper but for variable heights because it sucks at it
public sealed class VariableHeightClipper {
    private float?[] itemHeights = [];
    private float averageItemHeight;
    private int measuredItemCount;

    public void Draw(int itemCount, Action<int> drawItem) {
        if (itemHeights.Length != itemCount) {
            itemHeights = new float?[itemCount];
            averageItemHeight = 0f;
            measuredItemCount = 0;
        }

        if (itemCount == 0) return;

        var listTop = ImGui.GetCursorPosY();
        var scrollY = ImGui.GetScrollY();
        var scrollTop = Math.Max(0f, scrollY - listTop);
        var scrollBottom = scrollY + ImGui.GetContentRegionAvail().Y;

        var start = FindFirstVisible(scrollTop, out var skipAbove);
        if (skipAbove > 0) {
            ImGui.Dummy(new Vector2(0, skipAbove));
        }

        var end = DrawVisible(start, skipAbove, scrollBottom, itemCount, drawItem);

        var skipBelow = 0f;
        for (var i = end; i < itemCount; i++) {
            skipBelow += EstimateHeight(i);
        }

        if (skipBelow > 0) {
            ImGui.Dummy(new Vector2(0, skipBelow));
        }
    }

    private int FindFirstVisible(float scrollTop, out float skipAbove) {
        skipAbove = 0f;

        for (var i = 0; i < itemHeights.Length; i++) {
            var estimate = EstimateHeight(i);
            if (skipAbove + estimate >= scrollTop) {
                return i;
            }

            skipAbove += estimate;
        }

        var last = itemHeights.Length - 1;
        skipAbove -= EstimateHeight(last);
        return last;
    }

    private int DrawVisible(int start, float skipAbove, float scrollBottom, int itemCount, Action<int> drawItem) {
        var cursor = skipAbove;

        for (var i = start; i < itemCount; i++) {
            if (cursor >= scrollBottom) return i;

            var itemTop = ImGui.GetCursorPosY();
            drawItem(i);
            var height = ImGui.GetCursorPosY() - itemTop;

            RecordHeight(i, height);
            cursor += height;
        }

        return itemCount;
    }

    private float EstimateHeight(int index) {
        if (itemHeights[index] is { } measured) return measured;
        return measuredItemCount > 0 ? averageItemHeight : ImGui.GetTextLineHeightWithSpacing() * 4f;
    }

    private void RecordHeight(int index, float height) {
        if (itemHeights[index] is null) {
            averageItemHeight = (averageItemHeight * measuredItemCount + height) / (measuredItemCount + 1);
            measuredItemCount++;
        }

        itemHeights[index] = height;
    }
}
