using MapQuest.Interfaces;
using MapQuest.Models;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace MapQuest.Data.Document;

internal class Executor(string _connectionString) : IDocumentRepositoryExecutor, IAsyncDisposable
{
    private SqliteConnection? _connection;
    private DbTransaction? _transaction;

    public async Task<FilteredDataResult<T>> GetDataAsync<T>(string tableName, FilteredDataRequest request) where T : DocumentModel, new()
    {
        var result = new FilteredDataResult<T>();
        var items = new List<T>();

        using var command = CreateCommand($"select Id, UserId, Lat, Lon, QuestId, InsertedAt, UpdatedAt, Data from {tableName}{(request.Take != null ? $" limit {request.Take}" : "")}{(request.Skip != null ? $" offset {request.Skip}" : "")}");
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetString(0);
            var userId = reader.GetString(1);
            var lat = reader.GetInt64(2);
            var lon = reader.GetInt64(3);
            var questId = reader.GetString(4);
            var insertedAt = reader.GetDateTime(5);
            var updatedAt = reader.GetDateTime(6);
            var data = reader.GetString(7);

            var record = DocumentModel.FromData<T>(id, userId, lat, lon, questId, insertedAt, updatedAt, data);
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
