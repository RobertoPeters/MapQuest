using MapQuest.Data;
using MapQuest.Interfaces;
using MapQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace MapQuest.UserService;

public class UserService(ApplicationDbContext _applicationDbContext) : IUserService
{
    public async Task<FilteredDataResult<User>> GetUsersAsync(FilteredDataRequest request)
    {
        var result = new FilteredDataResult<User>();
        result.Items = _applicationDbContext.Users
            .Skip(request.Skip ?? 0)
            .Take(request.Take ?? int.MaxValue)
            .Select(x => new Models.User
            {
                Id = x.Id,
                Username = x.UserName ?? "",
                Email = x.Email ?? ""
            });

        if (request.Skip == null && request.Take == null)
        {
            result.Count = result.Items.Count();
        }
        else
        {
            result.Count = await _applicationDbContext.Users.CountAsync();
        }

        return result;
    }
}
