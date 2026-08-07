using RslLibrary.Data;

namespace RslLibrary.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public ICopyRepository Copies { get; }
    public ILoanRepository Loans { get; }
    public IReservationRepository Reservations { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Copies = new CopyRepository(_context);
        Loans = new LoanRepository(_context);
        Reservations = new ReservationRepository(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}