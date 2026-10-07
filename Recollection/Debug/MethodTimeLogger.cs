#if DEBUG
using System;
using System.Reflection;

namespace Recollection.Debug;

internal static class MethodTimeLogger {
    public static void Log(MethodBase methodBase, TimeSpan elapsed, string message) {
        TimingStats.Record(methodBase, elapsed);
    }
}
#endif
