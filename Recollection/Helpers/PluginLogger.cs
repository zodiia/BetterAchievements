using System;
using Dalamud.Plugin.Services;
using Serilog;
using Serilog.Events;

namespace Recollection.Helpers;

public class PluginLogger<T>(IPluginLog parent) : IPluginLog {
    private readonly string name = FormatName(typeof(T).Name);

    private static string FormatName(string name) => name.Length switch {
        < 16 => name.PadRight(16, ' '),
        16 => name,
        > 16 => $"{name[..7]}..{name[^7..]}",
    };

    public void Fatal(string messageTemplate, params object[] values) =>
        parent.Fatal($"[{name}] " + messageTemplate, values);

    public void Fatal(Exception? exception, string messageTemplate, params object[] values) =>
        parent.Fatal(exception, $"[{name}] " + messageTemplate, values);

    public void Error(string messageTemplate, params object[] values) =>
        parent.Error($"[{name}] " + messageTemplate, values);

    public void Error(Exception? exception, string messageTemplate, params object[] values) =>
        parent.Error(exception, $"[{name}] " + messageTemplate, values);

    public void Warning(string messageTemplate, params object[] values) =>
        parent.Warning($"[{name}] " + messageTemplate, values);

    public void Warning(Exception? exception, string messageTemplate, params object[] values) =>
        parent.Warning(exception, $"[{name}] " + messageTemplate, values);

    public void Information(string messageTemplate, params object[] values) =>
        parent.Information($"[{name}] " + messageTemplate, values);

    public void Information(Exception? exception, string messageTemplate, params object[] values) =>
        parent.Information(exception, $"[{name}] " + messageTemplate, values);

    public void Info(string messageTemplate, params object[] values) =>
        parent.Info($"[{name}] " + messageTemplate, values);

    public void Info(Exception? exception, string messageTemplate, params object[] values) =>
        parent.Info(exception, $"[{name}] " + messageTemplate, values);

    public void Debug(string messageTemplate, params object[] values) =>
        parent.Debug($"[{name}] " + messageTemplate, values);

    public void Debug(Exception? exception, string messageTemplate, params object[] values) =>
        parent.Debug(exception, $"[{name}] " + messageTemplate, values);

    public void Verbose(string messageTemplate, params object[] values) =>
        parent.Verbose($"[{name}] " + messageTemplate, values);

    public void Verbose(Exception? exception, string messageTemplate, params object[] values) =>
        parent.Verbose(exception, $"[{name}] " + messageTemplate, values);

    public void Write(LogEventLevel level, Exception? exception, string messageTemplate, params object[] values) =>
        parent.Write(level, exception, $"[{name}] " + messageTemplate, values);

    public ILogger Logger => parent.Logger;
    public LogEventLevel MinimumLogLevel {
        get => parent.MinimumLogLevel;
        set => parent.MinimumLogLevel = value;
    }
}
