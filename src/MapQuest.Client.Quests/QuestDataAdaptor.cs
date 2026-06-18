using MapQuest.Client.Interfaces;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace MapQuest.Client.Quests;

public class QuestDataAdaptor(IQuestService _questService, MapQuest.Interfaces.IGpsService _gpsService) : DataAdaptor
{
    public override async Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string? additionalParam = null)
    {
        var userDataResult = await _questService.GetUserQuestsAsync(new Models.FilteredDataRequest() 
        {
            Skip = dataManagerRequest.Skip,
            Take = dataManagerRequest.Take,
            Lat = _gpsService.CurrentLocation?.Latitude,
            Lon = _gpsService.CurrentLocation?.Longitude,
        }, null);

        var result = new DataResult()
        {
            Count = userDataResult.Count,
            Result = userDataResult.Items
        };
        
        if (userDataResult.Distances != null)
        {
            foreach (var record in userDataResult.Items)
            {
                record.CalculatedDistance = userDataResult.Distances[record.Id];
            }
        }

        return result;
    }
}
