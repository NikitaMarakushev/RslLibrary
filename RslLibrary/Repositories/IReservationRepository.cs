using RslLibrary.Models;

namespace RslLibrary.Repositories;

public interface IReservationRepository : IGenericRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetActiveReservationsByUserAsync(int userId);
    Task<Reservation?> GetActiveReservationByCopyAndUserAsync(int copyId, int userId);
}