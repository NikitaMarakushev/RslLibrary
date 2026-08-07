using Microsoft.EntityFrameworkCore;
using RslLibrary.Data;
using RslLibrary.Constants;

namespace RslLibrary.Services;

public interface IStatisticsService
{
    Task<Dictionary<string, int>> GetBooksByCategoryAsync();
    Task<Dictionary<string, int>> GetLoansByMonthAsync();
    Task<Dictionary<string, int>> GetPopularBooksAsync();
    Task<int> GetTotalBooksCountAsync();
    Task<int> GetActiveLoansCountAsync();
    Task<int> GetOverdueLoansCountAsync();
}

public class StatisticsService : IStatisticsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public StatisticsService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Dictionary<string, int>> GetBooksByCategoryAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Books
            .Include(b => b.Category)
            .GroupBy(b => b.Category.Name)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetLoansByMonthAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var sixMonthsAgo = DateTime.Now.AddMonths(-5);
        var firstDayOfPeriod = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

        var loans = await context.Loans
            .Where(l => l.LoanDate >= DateOnly.FromDateTime(firstDayOfPeriod))
            .ToListAsync();

        var result = new Dictionary<string, int>();

        for (int i = 0; i < 6; i++)
        {
            var month = firstDayOfPeriod.AddMonths(i);
            var monthName = month.ToString("MMM yyyy");
            var count = loans.Count(l =>
                l.LoanDate.Year == month.Year &&
                l.LoanDate.Month == month.Month);

            result[monthName] = count;
        }

        return result;
    }

    public async Task<Dictionary<string, int>> GetPopularBooksAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var loans = await context.Loans
            .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
            .GroupBy(l => l.Copy.Book.Title)
            .Select(g => new { Title = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        return loans.ToDictionary(x => x.Title, x => x.Count);
    }

    public async Task<int> GetTotalBooksCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Books.CountAsync();
    }

    public async Task<int> GetActiveLoansCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Loans
            .CountAsync(l => l.StatusId == LoanStatuses.Active);
    }

    public async Task<int> GetOverdueLoansCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var today = DateOnly.FromDateTime(DateTime.Now);

        return await context.Loans
            .CountAsync(l => l.StatusId == LoanStatuses.Active && l.DueDate < today);
    }
}