using MapQuest.Interfaces;
using MapQuest.Models;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace MapQuest.Data.Document;

internal class Executor(string _connectionString) : IDocumentRepositoryExecutor, IAsyncDisposable
{
    private SqliteConnection? _connection;
    private DbTransaction? _transaction;

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
