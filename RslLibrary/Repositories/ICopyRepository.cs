using RslLibrary.Models;

namespace RslLibrary.Repositories;

public interface ICopyRepository : IGenericRepository<BookCopy>
{
    Task<BookCopy?> GetByInventoryNumberAsync(string inventoryNumber);
    Task<int> GetAvailableCopiesCountAsync(int bookId);
}