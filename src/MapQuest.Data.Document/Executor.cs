using MapQuest.Interfaces;
using MapQuest.Models;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace MapQuest.Data.Document;

internal class Executor(string _connectionString) : IDocumentRepositoryExecutor, IAsyncDisposable
{
    private SqliteConnection? _connection;
    private DbTransaction? _transaction;
    private static HashSet<string> _validTableNames = [];
    private static HashSet<string> _validColumnNames = [];

    static Executor()
    {
        List<string> tableNames = Enum.GetNames<UserDatabaseTables>().ToList();
        tableNames.AddRange(Enum.GetNames<GlobalDatabaseTables>().ToList());
        _validTableNames = [.. tableNames.Select(x => x.ToLower()).Distinct()];

        var columnMembers = typeof(DocumentModel).GetProperties().Select(x => x.Name).ToList();
        _validColumnNames = [.. columnMembers.Select(x => x.ToLower()).Distinct()];
    }

    public async Task<FilteredDataResult<T>> GetDataAsync<T>(string tableName, FilteredDataRequest request) where T : DocumentModel, new()
    {
        if (!_validTableNames.Contains(tableName.ToLower()))
        {
            throw new ArgumentException("Invalid table name", nameof(tableName));
        }

        var result = new FilteredDataResult<T>();
        var items = new List<T>();

        var whereClauseData = CreateWhereClause(request.Filter);
        using var command = CreateCommand($"select Data from {tableName}{whereClauseData.whereClause}{(request.Take != null ? $" limit {request.Take}" : "")}{(request.Skip != null ? $" offset {request.Skip}" : "")}");
        foreach (var parameter in whereClauseData.parameters)
        {
            command.Parameters.Add(parameter);
        }
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
            using var countCommand = CreateCommand($"select count(*) from {tableName} {whereClauseData.whereClause}");
            foreach (var parameter in whereClauseData.parameters)
            {
                countCommand.Parameters.Add(parameter);
            }
            result.Count = Convert.ToInt32(await countCommand.ExecuteScalarAsync()!);
        }

        return result;
    }

    public async Task<int> DeleteDataAsync(string tableName, string columnName, object? value)
    {
        if (!_validTableNames.Contains(tableName.ToLower()))
        {
            throw new ArgumentException("Invalid table name", nameof(tableName));
        }

        if (!_validColumnNames.Contains(columnName.ToLower()))
        {
            throw new ArgumentException("Invalid column name", nameof(columnName));
        }

        var whereClauseData = CreateWhereClause([(columnName, value)]);
        using var command = CreateCommand($"delete from {tableName} {whereClauseData.whereClause}");
        foreach (var parameter in whereClauseData.parameters)
        {
            command.Parameters.Add(parameter);
        }
        return await command.ExecuteNonQueryAsync();
    }

    private (string whereClause, List<SqliteParameter> parameters) CreateWhereClause(IEnumerable<(string ColumnName, object? Value)>? filter)
    {
        if (filter == null || !filter.Any())
        {
            return (string.Empty, []);
        }
        var whereClauses = new List<string>();
        var parameters = new List<SqliteParameter>();
        foreach (var (columnName, value) in filter)
        {
            if (!_validColumnNames.Contains(columnName.ToLower()))
            {
                throw new ArgumentException($"Invalid column name: {columnName}", nameof(filter));
            }
            if (value == null)
            {
                whereClauses.Add($"{columnName} IS NULL");
            }
            else
            {
                whereClauses.Add($"{columnName} = @{columnName}");
                parameters.Add(new SqliteParameter(columnName, value));
            }
        }
        return (" WHERE " + string.Join(" AND ", whereClauses), parameters);
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
