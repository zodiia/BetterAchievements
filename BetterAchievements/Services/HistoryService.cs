using System;
using System.Collections.Generic;
using System.IO;
using BetterAchievements.Data.Unlockable;
using Dalamud.Plugin.Services;
using Dapper;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Data.Sqlite;
using Serilog;

namespace BetterAchievements.Services;

public record AchievementUpdate {
    public ulong Id { get; init; }
    public uint AchievementId { get; init; }
    public ulong Timestamp { get; init; }
    public bool Status { get; init; }
    public uint? Progress { get; init; }
}

public record AchievementStatus {
    public uint AchievementId { get; init; }
    public bool Status { get; init; }
    public uint? Progress { get; init; }
}

public class HistoryService : IDisposable {
    private readonly IPluginLog log = Plugin.GetLogger<HistoryService>();
    private readonly Plugin plugin;
    private readonly SqliteConnection connection;

    public HistoryService(Plugin plugin) {
        var path = Path.Combine(Plugin.PluginInterface.ConfigDirectory.FullName, "history.db");

        this.plugin = plugin;
        connection = new SqliteConnection($"Data Source={path}");
        connection.Open();
        connection.Execute("PRAGMA journal_mode=WAL;");

        Migrate();
        SetupEvents();
    }

    private unsafe void SetupEvents() {
        Plugin.UnlockState.Unlock += OnUnlock;
        plugin.ReceiveAchievementProgressHook.OnDetour += (_, id, current, max) => OnReceiveAchievementProgress(id, current, max);
    }

    private void Migrate() {
        var version = connection.ExecuteScalar<int>("PRAGMA user_version;");

        if (version < 1) {
            connection.Execute("""
            CREATE TABLE AchievementUpdate (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AchievementId INTEGER NOT NULL,
                Timestamp INTEGER NOT NULL,
                Status INTEGER,
                Progress INTEGER
            );

            CREATE TABLE AchievementStatus (
                AchievementId INTEGER PRIMARY KEY NOT NULL,
                Status INTEGER NOT NULL,
                Progress INTEGER
            );
            """);
            connection.Execute("PRAGMA user_version = 1;");
        }
    }

    public void UpdateAchievementStatus(AchievementStatus status) {
        var changed = connection.ExecuteScalar<uint?>(""" 
            INSERT INTO AchievementStatus (AchievementId, Status, Progress)
            VALUES (@AchievementId, @Status, @Progress)
            ON CONFLICT (AchievementId) DO UPDATE SET
                Status = excluded.Status,
                Progress = excluded.Progress
            WHERE AchievementStatus.Status IS NOT excluded.Status
               OR AchievementStatus.Progress IS NOT excluded.Progress
            RETURNING AchievementId;
            """, status);

        if (changed is null) {
            return;
        }

        connection.Execute("""
            INSERT INTO AchievementUpdate (AchievementId, Timestamp, Status, Progress)
            VALUES (@AchievementId, @Timestamp, @Status, @Progress);
            """, new {
                status.AchievementId,
                Timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                status.Status,
                status.Progress,
            });
    }

    public IEnumerable<AchievementStatus> GetAllAchievementStatus() {
        return connection.QueryMultiple("""
            SELECT AchievementId, Status, Progress
            FROM AchievementStatus
            """).Read<AchievementStatus>();
    }

    public AchievementStatus? GetAchievementStatus(uint achievementId) {
        return connection.QuerySingleOrDefault<AchievementStatus>("""
            SELECT AchievementId, Status, Progress
            FROM AchievementStatus
            WHERE AchievementId = @AchievementId;
            """, new { AchievementId = achievementId });
    }

    public List<AchievementUpdate> GetAchievementUpdates(uint achievementId) {
        return connection.Query<AchievementUpdate>("""
            SELECT Id, AchievementId, Timestamp, Status, Progress
            FROM AchievementUpdate
            WHERE AchievementId = @AchievementId;
            """, new { AchievementId = achievementId }).AsList();
    }

    private void OnUnlock(RowRef rowRef) {
        if (!rowRef.TryGetValue(out Achievement achievement)) {
            log.Warning($"Could not find achievement with RowRef.RowId = {rowRef.RowId}.");
        }
        UpdateAchievementStatus(new AchievementStatus { AchievementId = achievement.RowId, Status = true, Progress = achievement.Maximum() });
    }

    private void OnReceiveAchievementProgress(uint id, uint current, uint max) {
        UpdateAchievementStatus(new AchievementStatus { AchievementId = id, Progress = current, Status = current == max });
    }

    public void Dispose() {
        connection.Dispose();
    }
}
