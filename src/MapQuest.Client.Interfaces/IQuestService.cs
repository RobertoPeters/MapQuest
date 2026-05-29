using MapQuest.Models;
using Refit;
using System.Diagnostics.CodeAnalysis;

namespace MapQuest.Client.Interfaces;

public interface IQuestService
{
    [Get("/api/quest")]
    Task<FilteredDataResult<User>> GetUserQuestsAsync([Query] FilteredDataRequest request, string? userId = null);
}
