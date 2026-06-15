using MapQuest.Models;

namespace MapQuest.Interfaces;

public interface IQuestService
{
    Task<FilteredDataResult<Quest>> GetUserQuestsAsync(FilteredDataRequest request, string userId);
    Task AddQuestAsync(string userId, Quest quest, QuestDescription questDescription);
}
