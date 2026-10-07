#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;

namespace Recollection.Debug;

public sealed class TimingsWindow : Window, IDisposable {
    private const string Command = "/rectimings";

    private static readonly string[] Columns = ["Method", "Calls", "Total (ms)", "Median (ms)", "Average (ms)", "P95 (ms)", "P99 (ms)", "Max (ms)"];

    private static TimingsWindow? Instance;

    private readonly WindowSystem windowSystem = new("Recollection.Timings");
    private string search = "";
    private string sampleWindow = TimingStats.DefaultSampleWindow.ToString();

    private TimingsWindow() : base("Timings") {
        Size = new Vector2(700, 400);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public static void Initialize() {
        Instance = new TimingsWindow();
        Instance.windowSystem.AddWindow(Instance);
        Plugin.PluginInterface.UiBuilder.Draw += Instance.windowSystem.Draw;
        Plugin.CommandManager.AddHandler(Command, new CommandInfo((_, _) => Instance.Toggle()) { HelpMessage = "Toggle the timings debug window" });
    }

    public static void Shutdown() {
        if (Instance == null) return;
        Plugin.CommandManager.RemoveHandler(Command);
        Plugin.PluginInterface.UiBuilder.Draw -= Instance.windowSystem.Draw;
        Instance.Dispose();
        Instance = null;
    }

    public void Dispose() => windowSystem.RemoveAllWindows();

    public override void Draw() {
        ImGui.InputTextWithHint("##Search", "Search...", ref search, 128);
        ImGui.SameLine();

        if (ImGui.Button("Clear")) {
            TimingStats.Clear();
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(80);
        ImGui.InputText("Sample window", ref sampleWindow, 10);
        TimingStats.SampleWindow = int.TryParse(sampleWindow, out var window) ? window : TimingStats.DefaultSampleWindow;

        if (!ImGui.BeginTable("Timings", Columns.Length, ImGuiTableFlags.Sortable | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY)) {
            return;
        }

        for (var i = 0; i < Columns.Length; i++) {
            ImGui.TableSetupColumn(Columns[i], i == 0 ? ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.DefaultSort : ImGuiTableColumnFlags.WidthFixed);
        }
        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableHeadersRow();

        IEnumerable<TimingRow> rows = TimingStats.Snapshot().Where(r => r.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        var specs = ImGui.TableGetSortSpecs().Specs;
        var column = specs.ColumnIndex;
        var descending = specs.SortDirection == ImGuiSortDirection.Descending;

        Func<TimingRow, IComparable> key = column switch {
            1 => r => r.Calls,
            2 => r => r.Total,
            3 => r => r.Median,
            4 => r => r.Average,
            5 => r => r.P95,
            6 => r => r.P99,
            7 => r => r.Max,
            _ => r => r.Name,
        };
        rows = descending ? rows.OrderByDescending(key) : rows.OrderBy(key);

        foreach (var row in rows) {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(row.Name);
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(row.Calls.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(row.Total.ToString("F3"));
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(row.Median.ToString("F3"));
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(row.Average.ToString("F3"));
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(row.P95.ToString("F3"));
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(row.P99.ToString("F3"));
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(row.Max.ToString("F3"));
        }

        ImGui.EndTable();
    }
}
#endif
