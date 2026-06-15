using MapQuest.Models;

namespace MapQuest.Interfaces;

public interface IDocumentRepositoryExecutor
{
    Task<FilteredDataResult<T>> GetDataAsync<T>(string tableName, FilteredDataRequest request) where T : DocumentModel, new();
    Task<int> InsertDataAsync<T>(string tableName, T data) where T : DocumentModel;
}
