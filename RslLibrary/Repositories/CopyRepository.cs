using Microsoft.EntityFrameworkCore;
using RslLibrary.Data;
using RslLibrary.Models;

namespace RslLibrary.Repositories;

public class CopyRepository : GenericRepository<BookCopy>, ICopyRepository
{
    public CopyRepository(ApplicationDbContext context) : base(context) { }

    public async Task<BookCopy?> GetByInventoryNumberAsync(string inventoryNumber)
    {
        return await _dbSet
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.InventoryNumber == inventoryNumber);
    }

    public async Task<int> GetAvailableCopiesCountAsync(int bookId)
    {
        return await _dbSet
            .CountAsync(c => c.BookId == bookId && c.StatusId == Constants.CopyStatuses.Available);
    }
}