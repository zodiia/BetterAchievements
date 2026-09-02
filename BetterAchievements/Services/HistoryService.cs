using System;
using System.Collections.Generic;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BetterAchievements.Services;

public record AchievementUpdate {
    public ulong Id;
    public uint AchievementId;
    public ulong Timestamp;
    public bool Status;
    public uint? Progress;
}

public record AchievementStatus {
    public uint AchievementId;
    public bool Status;
    public uint? Progress;
}

public class HistoryService : IDisposable {
    private readonly SqliteConnection connection;

    public HistoryService() {
        var path = Path.Combine(Plugin.PluginInterface.ConfigDirectory.FullName, "history.db");

        connection = new SqliteConnection($"Data Source={path}");
        connection.Open();
        connection.Execute("PRAGMA journal_mode=WAL;");

        Migrate();
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

    public void Dispose() {
        connection.Dispose();
    }
}
