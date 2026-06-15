using MapQuest.Interfaces;
using MapQuest.Models;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace MapQuest.Data.Document;

internal class Executor(string _connectionString) : IDocumentRepositoryExecutor, IAsyncDisposable
{
    private SqliteConnection? _connection;
    private DbTransaction? _transaction;
    private static HashSet<string> _validTableNames = [];

    static Executor()
    {
        List<string> tableNames = Enum.GetNames<UserDatabaseTables>().ToList();
        tableNames.AddRange(Enum.GetNames<GlobalDatabaseTables>().ToList());
        _validTableNames = [.. tableNames.Select(x => x.ToLower()).Distinct()];
    }

    public async Task<FilteredDataResult<T>> GetDataAsync<T>(string tableName, FilteredDataRequest request) where T : DocumentModel, new()
    {
        if (!_validTableNames.Contains(tableName.ToLower()))
        {
            throw new ArgumentException("Invalid table name", nameof(tableName));
        }

        var result = new FilteredDataResult<T>();
        var items = new List<T>();

        using var command = CreateCommand($"select Data from {tableName}{(request.Take != null ? $" limit {request.Take}" : "")}{(request.Skip != null ? $" offset {request.Skip}" : "")}");
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var data = reader.GetString(0);

            var record = DocumentModel.FromData<T>(data);
            items.Add(record);
        }

        result.Items = items;

        if (request.Take != null || request.Skip != null)
        {
            using var countCommand = CreateCommand($"select count(*) from {tableName}");
            result.Count = Convert.ToInt32(await countCommand.ExecuteScalarAsync()!);
        }

        return result;
    }

    public async Task<int> InsertDataAsync<T>(string tableName, T data) where T : DocumentModel
    {
        if (!_validTableNames.Contains(tableName.ToLower()))
        {
            throw new ArgumentException("Invalid table name", nameof(tableName));
        }

        if (string.IsNullOrWhiteSpace(data.Id))
        {
            data.Id = data.NewId();
        }

        using var command = CreateCommand($"insert into {tableName} (Id, UserId, Lat, Lon, QuestId, InsertedAt, UpdatedAt, Data) values (@Id, @UserId, @Lat, @Lon, @QuestId, @InsertedAt, @UpdatedAt, @Data)");
        command.Parameters.AddWithValue("@Id", data.Id);
        command.Parameters.AddWithValue("@UserId", data.UserId);
        if (data.Lat != null)
        {
            command.Parameters.AddWithValue("@Lat", data.Lat);
        }
        else
        {
            command.Parameters.AddWithValue("@Lat", DBNull.Value);
        }
        if (data.Lon != null)
        {
            command.Parameters.AddWithValue("@Lon", data.Lon);
        }
        else
        {
            command.Parameters.AddWithValue("@Lon", DBNull.Value);
        }
        command.Parameters.AddWithValue("@QuestId", data.QuestId);
        command.Parameters.AddWithValue("@InsertedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@UpdatedAt", DBNull.Value);
        command.Parameters.AddWithValue("@Data", data.ToData());
        return await command.ExecuteNonQueryAsync();
    }

    private SqliteCommand CreateCommand(string commandText)
    {
        var result = _connection!.CreateCommand();
        result.CommandText = commandText;
        if (_transaction != null)
        {
            result.Transaction = _transaction as SqliteTransaction;
        }
        return result;
    }

    public async Task TaskStartAsync(bool withinTransaction)
    {
        _connection = new SqliteConnection(_connectionString);
        await _connection.OpenAsync();
        if (withinTransaction)
        {
            _transaction = await _connection.BeginTransactionAsync();
        }
    }

    public async Task CancelTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await CommitTransactionAsync();

            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
