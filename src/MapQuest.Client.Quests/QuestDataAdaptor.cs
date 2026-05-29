using MapQuest.Client.Interfaces;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace MapQuest.Client.Quests;

public class QuestDataAdaptor(IQuestService _questService) : DataAdaptor
{
    public override async Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string? additionalParam = null)
    {
        var userDataResult = await _questService.GetUserQuestsAsync(new Models.FilteredDataRequest() 
        {
            Skip = dataManagerRequest.Skip,
            Take = dataManagerRequest.Take,
        }, null);

        var result = new DataResult()
        {
            Count = userDataResult.Count,
            Result = userDataResult.Items
        }; 

        return result;
    }
}
