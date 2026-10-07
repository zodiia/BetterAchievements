using MethodTimer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Recollection.Data.Unlockable;
using Recollection.Helpers;
using Dalamud.Plugin.Services;
using Dapper;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Data.Sqlite;

namespace Recollection.Services;

public record AchievementUpdate {
    public ulong CharacterId { get; init; }
    public ulong Id { get; init; }
    public uint AchievementId { get; init; }
    public ulong Timestamp { get; init; }
    public bool Status { get; init; }
    public uint? Progress { get; init; }
    public bool Imported { get; init; }
}

public record AchievementStatus {
    public ulong CharacterId { get; init; }
    public uint AchievementId { get; init; }
    public bool Status { get; init; }
    public uint? Progress { get; init; }
}

public sealed class HistoryService : IDisposable {
    private readonly IPluginLog log = Plugin.GetLogger<HistoryService>();
    private readonly Plugin plugin;
    private readonly SqliteConnection connection;

    [Time]
    public HistoryService(Plugin plugin) {
        var path = Path.Combine(Plugin.PluginInterface.ConfigDirectory.FullName, "history.db");

        this.plugin = plugin;
        connection = new SqliteConnection($"Data Source={path}");
        connection.Open();
        connection.Execute("PRAGMA journal_mode=WAL;");

        Migrate();
        SetupEvents();
    }

    private bool importedAchievementList;

    private unsafe void SetupEvents() {
        Plugin.UnlockState.Unlock += OnUnlock;
        plugin.ReceiveAchievementProgressHook.OnDetour += (_, id, current, max) => OnReceiveAchievementProgress(id, current, max);
        Plugin.Framework.Update += OnFrameworkUpdate;
        Plugin.ClientState.Login += OnLogin;
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

        if (version < 2) {
            connection.Execute("ALTER TABLE AchievementUpdate ADD COLUMN Imported INTEGER NOT NULL DEFAULT 0;");
            connection.Execute("PRAGMA user_version = 2;");
        }

        if (version < 3) {
            connection.Execute("""
                               CREATE TABLE AchievementStatus_new (
                                   CharacterId   INTEGER NOT NULL,
                                   AchievementId INTEGER NOT NULL,
                                   Status        INTEGER NOT NULL,
                                   Progress      INTEGER,
                                   PRIMARY KEY (CharacterId, AchievementId)
                               );

                               INSERT INTO AchievementStatus_new (CharacterId, AchievementId, Status, Progress)
                               SELECT 0, AchievementId, Status, Progress
                               FROM AchievementStatus;

                               DROP TABLE AchievementStatus;
                               ALTER TABLE AchievementStatus_new RENAME TO AchievementStatus;
                               """);
            connection.Execute("ALTER TABLE AchievementUpdate ADD COLUMN CharacterId INTEGER NOT NULL DEFAULT 0;");
            connection.Execute("PRAGMA user_version = 3;");
        }
    }

    [Time]
    public void UpdateAchievementStatus(AchievementStatus status) {
        var changed = connection.ExecuteScalar<uint?>(""" 
                                                      INSERT INTO AchievementStatus (CharacterId, AchievementId, Status, Progress)
                                                      VALUES (@CharacterId, @AchievementId, @Status, @Progress)
                                                      ON CONFLICT (CharacterId, AchievementId) DO UPDATE SET
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
                           INSERT INTO AchievementUpdate (CharacterId, AchievementId, Timestamp, Status, Progress)
                           VALUES (@CharacterId, @AchievementId, @Timestamp, @Status, @Progress);
                           """, new {
            status.CharacterId,
            status.AchievementId,
            Timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            status.Status,
            status.Progress,
        });
    }

    [Time]
    public void ImportAchievementStatuses(List<AchievementStatus> statuses) {
        var values = string.Join("), (", statuses.Select(s => $"{s.CharacterId}, {s.AchievementId}, {s.Status}, {s.Progress}"));
        var changedIds = connection.Query<uint>($"""
                                                INSERT INTO AchievementStatus (CharacterId, AchievementId, Status, Progress)
                                                VALUES ({values})
                                                ON CONFLICT (CharacterId, AchievementId) DO UPDATE SET
                                                    Status = excluded.Status,
                                                    Progress = excluded.Progress
                                                WHERE AchievementStatus.Status IS NOT excluded.Status
                                                   OR AchievementStatus.Progress IS NOT excluded.Progress
                                                RETURNING AchievementId
                                                """).AsList();

        if (changedIds.Count == 0) {
            return;
        }

        var timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        values = string.Join("), (", statuses.Where(it => changedIds.Contains(it.AchievementId))
                                           .Select(s => $"{s.CharacterId}, {s.AchievementId}, {timestamp}, {s.Status}, {s.Progress}, 1"));
        connection.Execute($"""
                           INSERT INTO AchievementUpdate (CharacterId, AchievementId, Timestamp, Status, Progress, Imported)
                           VALUES ({values})
                           """);
    }

    public IEnumerable<AchievementStatus> GetAllAchievementStatus(ulong characterId) {
        return connection.QueryMultiple("""
                                        SELECT AchievementId, Status, Progress
                                        FROM AchievementStatus
                                        WHERE CharacterId = @CharacterId
                                        """, new { CharacterId = characterId }).Read<AchievementStatus>();
    }

    [Time]
    public AchievementStatus? GetAchievementStatus(uint achievementId, ulong characterId) {
        return connection.QuerySingleOrDefault<AchievementStatus>("""
                                                                  SELECT CharacterId, AchievementId, Status, Progress
                                                                  FROM AchievementStatus
                                                                  WHERE AchievementId = @AchievementId AND CharacterId = @CharacterId;
                                                                  """, new { AchievementId = achievementId, CharacterId = characterId });
    }

    [Time]
    public List<AchievementUpdate> GetAchievementUpdates(uint achievementId, ulong characterId) {
        return connection.Query<AchievementUpdate>("""
                                                   SELECT CharacterId, Id, AchievementId, Timestamp, Status, Progress, Imported
                                                   FROM AchievementUpdate
                                                   WHERE AchievementId = @AchievementId AND CharacterId = @CharacterId;
                                                   """, new { AchievementId = achievementId, CharacterId = characterId }).AsList();
    }

    [Time]
    public List<AchievementUpdate> GetLastUnlockedAchievements(ulong characterId) {
        return connection.Query<AchievementUpdate>("""
                                                   SELECT CharacterId, Id, AchievementId, Timestamp, Status, Progress, Imported
                                                   FROM (
                                                       SELECT *, ROW_NUMBER() OVER (PARTITION BY AchievementId ORDER BY Timestamp DESC, Id DESC) AS RowNum
                                                       FROM AchievementUpdate
                                                       WHERE Status = 1 AND Imported = 0 AND CharacterId = @CharacterId
                                                   )
                                                   WHERE RowNum = 1
                                                   ORDER BY Timestamp DESC, Id DESC
                                                   LIMIT 10;
                                                   """, new { CharacterId = characterId }).AsList();
    }

    private void OnUnlock(RowRef rowRef) {
        if (!rowRef.TryGetValue(out Achievement achievement)) {
            log.Warning($"Could not find achievement with RowRef.RowId = {rowRef.RowId}.");
        }

        UpdateAchievementStatus(new AchievementStatus { CharacterId = Plugin.PlayerState.ContentId, AchievementId = achievement.RowId, Status = true, Progress = achievement.Maximum() });
    }

    private void OnReceiveAchievementProgress(uint id, uint current, uint max) {
        UpdateAchievementStatus(new AchievementStatus { CharacterId = Plugin.PlayerState.ContentId, AchievementId = id, Progress = current, Status = current == max });
    }

    [Time]
    private void OnFrameworkUpdate(IFramework framework) {
        if (importedAchievementList || !Plugin.UnlockState.IsAchievementListLoaded) {
            return;
        }

        importedAchievementList = true;
        Plugin.Framework.Update -= OnFrameworkUpdate;

        var statuses = ExcelSheets.Achievement.Value
                                  .Where(Plugin.UnlockState.IsAchievementComplete)
                                  .Select(achievement => new AchievementStatus
                                              { CharacterId = Plugin.PlayerState.ContentId, AchievementId = achievement.RowId, Status = true, Progress = achievement.Maximum() })
                                  .ToList();

        try {
            ImportAchievementStatuses(statuses);
            log.Information("Imported achievement progress from history database");
        } catch (Exception ex) {
            log.Warning(ex, "Failed to import achievement progress from history database");
        }
    }

    [Time]
    private void OnLogin() {
        var characterId = Plugin.PlayerState.ContentId;
        var tx = connection.BeginTransaction();

        try {
            var rows = 0;
            rows += connection.Execute("UPDATE AchievementStatus SET CharacterId = @CharacterId WHERE CharacterId = 0;", new { CharacterId = characterId });
            rows += connection.Execute("UPDATE AchievementUpdate SET CharacterId = @CharacterId WHERE CharacterId = 0;", new { CharacterId = characterId });
            tx.Commit();
            log.Information($"Updated {rows} rows with new character IDs");
        } catch (Exception ex) {
            tx.Rollback();
            log.Warning(ex, "Failed to update local database with character ID");
        }

        // we wanna re-import them on login
        importedAchievementList = false;
    }

    public void Dispose() {
        Plugin.Framework.Update -= OnFrameworkUpdate;
        Plugin.UnlockState.Unlock -= OnUnlock;
        Plugin.ClientState.Login -= OnLogin;
        connection.Dispose();
    }
}
