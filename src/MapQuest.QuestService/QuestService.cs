using MapQuest.Data.Document;
using MapQuest.Interfaces;
using MapQuest.Models;

namespace MapQuest.QuestService;

public class QuestService(IDocumentRepository _documentRepository) : IQuestService
{
    public async Task<FilteredDataResult<Quest>> GetUserQuestsAsync(FilteredDataRequest request, string userId)
    {
        FilteredDataResult<Quest> result = null!;
        await _documentRepository.Execute(userId, false, async (executor) =>
        {
            result = await executor.GetDataAsync<Quest>(UserDatabaseTables.Quest.ToString(), request);
        });

        return result;
    }
}
