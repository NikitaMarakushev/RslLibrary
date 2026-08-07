namespace RslLibrary.Repositories;

public interface IUnitOfWork : IDisposable
{
    ICopyRepository Copies { get; }
    ILoanRepository Loans { get; }
    IReservationRepository Reservations { get; }
    Task<int> CompleteAsync();
}