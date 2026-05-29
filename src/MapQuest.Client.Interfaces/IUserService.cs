using MapQuest.Models;
using Refit;

namespace MapQuest.Client.Interfaces;

public interface IUserService
{
    [Get("/api/users")]
    Task<FilteredDataResult<User>> GetUsersAsync([Query] FilteredDataRequest request);

    [Post("/api/users/login")]
    Task LoginAsync([Body] LoginRequest request);

    [Get("/api/users/logout")]
    Task LogoutAsync();
}
