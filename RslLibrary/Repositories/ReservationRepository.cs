using Microsoft.EntityFrameworkCore;
using RslLibrary.Data;
using RslLibrary.Models;

namespace RslLibrary.Repositories;

public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Reservation>> GetActiveReservationsByUserAsync(int userId)
    {
        return await _dbSet
            .Include(r => r.Copy)
            .ThenInclude(c => c.Book)
            .Where(r => r.UserId == userId &&
                       (r.StatusId == Constants.ReservationStatuses.Waiting ||
                        r.StatusId == Constants.ReservationStatuses.Ready))
            .ToListAsync();
    }

    public async Task<Reservation?> GetActiveReservationByCopyAndUserAsync(int copyId, int userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.CopyId == copyId
                                   && r.UserId == userId
                                   && r.StatusId == Constants.ReservationStatuses.Waiting);
    }
}