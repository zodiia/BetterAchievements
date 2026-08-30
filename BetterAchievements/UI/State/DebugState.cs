using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BetterAchievements.UI.State;

public class DebugState(Configuration configuration) {
    private const int HistorySize = 100;

    private readonly Queue<double> frameTimesMs = new(HistorySize);
    private readonly Stopwatch stopwatch = new();

    public double AverageMs { get; private set; }
    public double WorstMs { get; private set; }

    public void StartDebug() {
        if (!configuration.DebugMode) return;
        stopwatch.Restart();
    }

    public void EndDebug() {
        if (!configuration.DebugMode) return;
        stopwatch.Stop();

        if (frameTimesMs.Count == HistorySize) frameTimesMs.Dequeue();
        frameTimesMs.Enqueue(stopwatch.Elapsed.TotalMilliseconds);

        AverageMs = frameTimesMs.Average();
        WorstMs = frameTimesMs.Max();
    }
}
