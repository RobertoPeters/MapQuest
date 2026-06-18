using MapQuest.Interfaces;
using MapQuest.Models;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.Reflection;

namespace MapQuest.Data.Document;

internal class Executor(string _connectionString) : IDocumentRepositoryExecutor, IAsyncDisposable
{
    private SqliteConnection? _connection;
    private DbTransaction? _transaction;
    private static HashSet<string> _validTableNames = [];
    private static Dictionary<string, PropertyInfo> _validColumns = [];
    private static Dictionary<string, PropertyInfo> _realTableColumns = [];

    static Executor()
    {
        List<string> tableNames = Enum.GetNames<UserDatabaseTables>().ToList();
        tableNames.AddRange(Enum.GetNames<GlobalDatabaseTables>().ToList());
        _validTableNames = [.. tableNames.Distinct()];

        _validColumns = typeof(DocumentModel).GetProperties().ToDictionary(x => x.Name, x => x);
        _realTableColumns = _validColumns.Where(x => x.Key != "CalculatedDistance").ToDictionary(x => x.Key, x => x.Value);
    }

    public async Task<FilteredDataResult<T>> GetDataAsync<T>(string tableName, FilteredDataRequest request) where T : DocumentModel, new()
    {
        if (!_validTableNames.Contains(tableName))
        {
            throw new ArgumentException("Invalid table name", nameof(tableName));
        }

        var result = new FilteredDataResult<T>();
        var items = new List<T>();
        var latLonProvided = request.Lat != null && request.Lon != null;

        var whereClauseData = CreateWhereClause(request.Filter);
        using var command = CreateCommand($"select Data {(latLonProvided ? ", Distance(@UserLat, @UserLon, Lat, Lon) as CalculatedDistance" : "")} from {tableName}{whereClauseData.whereClause}{(request.Take != null ? $" limit {request.Take}" : "")}{(request.Skip != null ? $" offset {request.Skip}" : "")}");
        if (latLonProvided)
        {
            result.Distances = [];
            command.Parameters.AddWithValue("UserLat",request.Lat);
            command.Parameters.AddWithValue("UserLon",request.Lon);
        }
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

            result.Distances?.Add(record.Id, reader.IsDBNull(1) ? null : reader.GetDouble(1));
        }

        result.Items = items;

        if (!request.IgnoreCount && (request.Take != null || request.Skip != null))
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
        if (!_validTableNames.Contains(tableName))
        {
            throw new ArgumentException("Invalid table name", nameof(tableName));
        }

        if (!_validColumns.ContainsKey(columnName))
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
            if (!_validColumns.ContainsKey(columnName))
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
        if (!_validTableNames.Contains(tableName))
        {
            throw new ArgumentException("Invalid table name", nameof(tableName));
        }

        if (string.IsNullOrWhiteSpace(data.Id))
        {
            data.Id = data.NewId();
        }

        data.InsertedAt = DateTime.UtcNow;

        using var command = CreateCommand($"insert into {tableName} ({string.Join(", ", _realTableColumns.Keys)}, Data) values (@{string.Join(", @", _realTableColumns.Keys)}, @Data)");
        foreach(var column in _realTableColumns)
        {
            var value = column.Value.GetValue(data);
            if (value != null)
            {
                command.Parameters.AddWithValue(column.Key, value);
            }
            else
            {
                command.Parameters.AddWithValue(column.Key, DBNull.Value);
            }
        }
        command.Parameters.AddWithValue("@Data", data.ToData());
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> UpdateDataAsync<T>(string tableName, T data) where T : DocumentModel
    {
        if (!_validTableNames.Contains(tableName))
        {
            throw new ArgumentException("Invalid table name", nameof(tableName));
        }

        data.UpdatedAt = DateTime.UtcNow;

        var allCollumnsWithoutId = _realTableColumns.Keys.Where(x => x != "Id").Select(x => $"{x} = @{x}").ToList();
        using var command = CreateCommand($"update {tableName} set {string.Join(", ", allCollumnsWithoutId)}, Data = @Data where Id = @Id");
        foreach (var column in _realTableColumns)
        {
            var value = column.Value.GetValue(data);
            if (value != null)
            {
                command.Parameters.AddWithValue(column.Key, value);
            }
            else
            {
                command.Parameters.AddWithValue(column.Key, DBNull.Value);
            }
        }
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
        _connection.CreateFunction("Distance", (double? lat1, double? lon1, double? lat2, double? lon2) => Geo.GeoService.CalculateDistance(lat1, lon1, lat2, lon2));
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
