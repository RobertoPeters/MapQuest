namespace MapQuest.Interfaces;

public interface IDocumentRepository
{
    Task SetupAsync();
    Task Execute(string? userId, bool withinTransaction, Func<IDocumentRepositoryExecutor, Task> actions);
}
