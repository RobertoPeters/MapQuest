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

    public async Task AddQuestAsync(string userId, Quest quest, QuestDescription questDescription)
    {
        await _documentRepository.Execute(userId, true, async (executor) =>
        {
            quest.UserId = userId;
            quest.Id = quest.NewId();
            quest.QuestId = quest.Id;
            questDescription.Id = quest.Id;
            questDescription.QuestId = quest.Id;
            questDescription.UserId = userId;

            await executor.InsertDataAsync(UserDatabaseTables.Quest.ToString(), quest);
            await executor.InsertDataAsync(UserDatabaseTables.QuestDescription.ToString(), questDescription);
        });
    }
}
