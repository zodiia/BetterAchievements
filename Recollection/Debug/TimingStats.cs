#if DEBUG
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Recollection.Debug;

public readonly record struct TimingRow(string Name, long Calls, double Total, double Median, double Average, double P95, double P99, double Max);

public static class TimingStats {
    public const int DefaultSampleWindow = 10000;

    // Percentiles are computed over the most recent SampleWindow calls of each method.
    public static int SampleWindow { get; set; } = DefaultSampleWindow;

    private sealed class Entry {
        public readonly object Lock = new();
        public readonly List<double> Samples = [];
        public long Calls;
        public double Total;
        public double Max;
        public TimingRow? Cached;
        public int CachedWindow;
    }

    private static readonly ConcurrentDictionary<string, Entry> Entries = new();
    private static readonly ConcurrentDictionary<MethodBase, string> Names = new();

    public static void Record(MethodBase method, TimeSpan elapsed) {
        var name = Names.GetOrAdd(method, FormatName);
        var ms = elapsed.TotalMilliseconds;
        var entry = Entries.GetOrAdd(name, _ => new Entry());

        lock (entry.Lock) {
            var index = (int)(entry.Calls % SampleWindow);
            if (index < entry.Samples.Count) entry.Samples[index] = ms;
            else entry.Samples.Add(ms);
            entry.Calls++;
            entry.Total += ms;
            if (ms > entry.Max) entry.Max = ms;
            entry.Cached = null;
        }
    }

    // Type.Method(ParamType, ParamType), so overloads get their own row.
    private static string FormatName(MethodBase method) {
        var parameters = string.Join(", ", method.GetParameters().Select(p => FormatType(p.ParameterType)));
        return $"{method.DeclaringType?.Name}.{method.Name.TrimStart('.')}({parameters})";
    }

    private static string FormatType(Type type) {
        if (type.IsByRef || type.IsPointer) return FormatType(type.GetElementType()!) + (type.IsByRef ? "&" : "*");
        if (type.IsArray) return FormatType(type.GetElementType()!) + "[]";
        if (!type.IsGenericType) return type.Name;
        return $"{type.Name[..type.Name.IndexOf('`')]}<{string.Join(", ", type.GetGenericArguments().Select(FormatType))}>";
    }

    public static void Clear() => Entries.Clear();

    public static List<TimingRow> Snapshot() {
        var rows = new List<TimingRow>(Entries.Count);
        foreach (var (name, entry) in Entries) {
            lock (entry.Lock) {
                if (entry.Cached == null || entry.CachedWindow != SampleWindow) {
                    var sorted = entry.Samples.Take(SampleWindow).Order().ToArray();
                    entry.Cached = new TimingRow(name, entry.Calls, entry.Total, Percentile(sorted, 0.5), entry.Total / entry.Calls, Percentile(sorted, 0.95), Percentile(sorted, 0.99), entry.Max);
                    entry.CachedWindow = SampleWindow;
                }
                rows.Add(entry.Cached.Value);
            }
        }
        return rows;
    }

    private static double Percentile(double[] sorted, double p) => sorted[Math.Max(0, (int)Math.Ceiling(p * sorted.Length) - 1)];
}
#endif
