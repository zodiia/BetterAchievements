using System;
using System.Collections.Generic;
using System.Linq;
using Recollection.UI.Component;
using Recollection.UI.State;

namespace Recollection.UI.Windows.Views.Roulette;

public class RouletteRoll {
    public static readonly int[] CardCounts = [1, 2, 4, 6, 8];

    public List<AchievementPoolEntry> Cards { get; } = [];
    public double[] CardRevealedAt { get; } = new double[CardCounts[^1]];
    public bool IsOpen { get; set; }
    public double RerollStartedAt { get; set; } = -1;
    public float CategoryTreeHeight { get; set; } = UiSize.Em(10f);

    public void Draw(List<AchievementPoolEntry> candidates, int count) {
        Cards.Clear();
        Cards.AddRange(candidates.OrderBy(_ => Random.Shared.Next()).Take(count));
        Array.Fill(CardRevealedAt, -1);
    }

    public void Close() {
        RerollStartedAt = -1;
        Cards.Clear();
        IsOpen = false;
    }
}
