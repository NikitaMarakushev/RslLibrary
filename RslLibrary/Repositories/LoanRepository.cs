using Microsoft.EntityFrameworkCore;
using RslLibrary.Data;
using RslLibrary.Models;

namespace RslLibrary.Repositories;

public class LoanRepository : GenericRepository<Loan>, ILoanRepository
{
    public LoanRepository(ApplicationDbContext context) : base(context) { }

    public async Task<int> GetActiveLoansCountByUserAsync(int userId)
    {
        return await _dbSet
            .CountAsync(l => l.UserId == userId && l.StatusId == Constants.LoanStatuses.Active);
    }

    public async Task<bool> HasUserOverdueLoansAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _dbSet
            .AnyAsync(l => l.UserId == userId
                        && l.StatusId == Constants.LoanStatuses.Active
                        && l.DueDate < today);
    }

    public async Task<Loan?> GetActiveLoanByCopyIdAsync(int copyId)
    {
        return await _dbSet
            .Include(l => l.Copy)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.CopyId == copyId && l.StatusId == Constants.LoanStatuses.Active);
    }

    public async Task<IEnumerable<Loan>> GetActiveLoansByUserAsync(int userId)
    {
        return await _dbSet
            .Include(l => l.Copy)
            .ThenInclude(c => c.Book)
            .Where(l => l.UserId == userId && l.StatusId == Constants.LoanStatuses.Active)
            .ToListAsync();
    }
}