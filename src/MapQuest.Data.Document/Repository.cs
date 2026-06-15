using MapQuest.Interfaces;
using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;

namespace MapQuest.Data.Document;

public class Repository : IDocumentRepository
{
    private readonly ConcurrentDictionary<string, byte> _userDbSetuped = [];
    private readonly object _lockObject = new();

    public async Task SetupAsync()
    {
        SQLitePCL.Batteries.Init();
        await SetupGlobalDatabase();
    }


    private string GetConnectionString(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return $"Data Source=./Settings/global.db";
        }
        return $"Data Source=./Settings/user_{userId}.db";
    }

    public async Task Execute(string? userId, bool withinTransaction, Func<IDocumentRepositoryExecutor, Task> actions)
    {
        var connectionString = GetConnectionString(userId);
        if (userId != null)
        {
            if (!_userDbSetuped.ContainsKey(userId))
            {
                lock (_lockObject)
                {
                    if (!_userDbSetuped.ContainsKey(userId))
                    {
                        SetupUserDatabase(userId);
                        _userDbSetuped.TryAdd(userId, 0);
                    }
                }
            }
        }
        await using var executor = new Executor(connectionString);
        await executor.TaskStartAsync(withinTransaction);
        try
        {
            await actions(executor);
        }
        catch
        {
            if (withinTransaction)
            {
                await executor.CancelTransactionAsync();
            }
            throw;
        }
    }


    private void SetupUserDatabase(string userId)
    {
        using var connection = new SqliteConnection(GetConnectionString(userId));
        connection.Open();
        UpgradeUserDatabase(connection);
    }

    private void UpgradeUserDatabase(SqliteConnection connection)
    {
        var currentVersion = GetDatabaseVersion(connection);
        if (currentVersion == 0)
        {
            var tableNames = Enum.GetNames<UserDatabaseTables>();
            var commandText = string.Join(";\n", tableNames.Select(t => CreateTableStatement(t)));
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
            command.Dispose();

            currentVersion = 1;
            command = connection.CreateCommand();
            command.CommandText = $"PRAGMA user_version = {currentVersion}";
            command.ExecuteNonQuery();
            command.Dispose();
        }
    }

    private string CreateTableStatement(string tableName)
    {
        return $"CREATE TABLE IF NOT EXISTS {tableName}(Id TEXT PRIMARY KEY, UserId TEXT, Lat INTEGER, Lon INTEGER, QuestId TEXT, InsertedAt INTEGER, UpdatedAt INTEGER, Data TEXT)";
    }


    private async Task SetupGlobalDatabase()
    {
        using var connection = new SqliteConnection(GetConnectionString(null));
        await connection.OpenAsync();
        await UpgradeGlobalDatabase(connection);
    }

    private async Task<long> UpgradeGlobalDatabase(SqliteConnection connection)
    {
        var currentVersion = await GetDatabaseVersionAsync(connection);
        if (currentVersion == 0)
        {
            var tableNames = Enum.GetNames<GlobalDatabaseTables>();
            var commandText = string.Join(";\n", tableNames.Select(t => CreateTableStatement(t)));
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
            await command.DisposeAsync();

            currentVersion = 1;
            command = connection.CreateCommand();
            command.CommandText = $"PRAGMA user_version = {currentVersion}";
            await command.ExecuteNonQueryAsync();
            await command.DisposeAsync();
        }
        return currentVersion;
    }

    private long GetDatabaseVersion(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version";
        var result = command.ExecuteScalar();
        if (result is long version)
        {
            return version;
        }
        return 0;
    }

    private async Task<long> GetDatabaseVersionAsync(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version";
        var result = await command.ExecuteScalarAsync();
        if (result is long version)
        {
            return version;
        }
        return 0;
    }
}
