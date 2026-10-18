using System;

namespace Recollection.Helpers;

public static class TimeFormat {
    public static string FormatTimeAgo(long timestamp) => FormatTimeAgo(DateTimeOffset.FromUnixTimeSeconds(timestamp));

    public static string FormatTimeAgo(DateTimeOffset time) {
        var elapsed = DateTimeOffset.UtcNow - time;

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
}
