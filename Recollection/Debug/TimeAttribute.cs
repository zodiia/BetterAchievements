#if !DEBUG
using System;
using System.Diagnostics;

namespace MethodTimer;

// replaces the attribute when in release mode with a no-op
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Assembly)]
[Conditional("DEBUG")]
internal sealed class TimeAttribute : Attribute  {
    public TimeAttribute() { }
    public TimeAttribute(string format) { }
}
#endif
