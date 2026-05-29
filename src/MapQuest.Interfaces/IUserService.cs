using MapQuest.Models;

namespace MapQuest.Interfaces;

public interface IUserService
{
    Task<FilteredDataResult<User>> GetUsersAsync(FilteredDataRequest request);
}
