using RslLibrary.Data;
using RslLibrary.Models;

namespace RslLibrary.Services;

public interface IUserService
{
    Task<ApplicationUser?> GetCurrentUserAsync();
    Task<IEnumerable<UserViewModel>> GetAllUsersWithRolesAsync();
}