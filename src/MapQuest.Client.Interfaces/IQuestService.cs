using MapQuest.Models;
using Refit;
using System.Diagnostics.CodeAnalysis;

namespace MapQuest.Client.Interfaces;

public interface IQuestService
{
    [Get("/api/quest")]
    Task<FilteredDataResult<Quest>> GetUserQuestsAsync([Query] FilteredDataRequest request, string? userId = null);

    [Post("/api/quest/add")]
    Task<Quest> AddQuestAsync([Body] QuestAndQuestDescription quest);

    [Post("/api/quest/update")]
    Task<Quest> UpdateQuestAsync([Body] QuestAndQuestDescription quest);

    [Delete("/api/quest/delete")]
    Task DeleteQuestsAsync(string questId, string? userId = null);

    [Get("/api/quest/description")]
    Task<QuestDescription?> GetQuestDescriptionAsync(string questId, string? userId = null);
}
