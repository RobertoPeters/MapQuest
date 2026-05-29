using MapQuest.Client.Interfaces;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace MapQuest.Client.Users;

public class UserDataAdaptor(IUserService _userService) : DataAdaptor
{
    public override async Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string? additionalParam = null)
    {
        var userDataResult = await _userService.GetUsersAsync(new Models.FilteredDataRequest() 
        {
            Skip = dataManagerRequest.Skip,
            Take = dataManagerRequest.Take,
        });

        var result = new DataResult()
        {
            Count = userDataResult.Count,
            Result = userDataResult.Items
        }; 

        return result;
    }
}
