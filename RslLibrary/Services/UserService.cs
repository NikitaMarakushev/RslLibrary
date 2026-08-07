using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RslLibrary.Data;
using RslLibrary.Models;

namespace RslLibrary.Services;

public class UserService : IUserService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        AuthenticationStateProvider authStateProvider,
        UserManager<ApplicationUser> userManager)
    {
        _contextFactory = contextFactory;
        _authStateProvider = authStateProvider;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var userId = int.Parse(user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Создаём НОВЫЙ контекст через фабрику для каждого запроса
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.FindAsync(userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ОШИБКА В GetCurrentUserAsync: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<UserViewModel>> GetAllUsersWithRolesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var users = await context.Users.OrderBy(u => u.FullName).ToListAsync();
        var result = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                ReaderTicketNumber = user.ReaderTicketNumber,
                IsActive = user.IsActive,
                Roles = roles.ToList()
            });
        }

        return result;
    }
}