using RslLibrary.Models;

namespace RslLibrary.Repositories;

public interface ILoanRepository : IGenericRepository<Loan>
{
    Task<int> GetActiveLoansCountByUserAsync(int userId);
    Task<bool> HasUserOverdueLoansAsync(int userId);
    Task<Loan?> GetActiveLoanByCopyIdAsync(int copyId);
    Task<IEnumerable<Loan>> GetActiveLoansByUserAsync(int userId);
}