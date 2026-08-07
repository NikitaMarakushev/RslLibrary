using RslLibrary.Models;
using RslLibrary.Services;

namespace RslLibrary.Services;

public interface IBookManagementService
{
    Task<(bool Success, string ErrorMessage)> AddBookAsync(CreateBookDto bookDto, string inventoryNumber, string? barcode, string? location);
    Task<bool> UpdateBookAsync(int bookId, CreateBookDto bookDto);
    Task<bool> DeleteBookAsync(int bookId);
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<IEnumerable<Author>> GetAllAuthorsAsync();
    Task<IEnumerable<Publisher>> GetAllPublishersAsync();
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<CreateBookDto?> GetBookForEditAsync(int bookId);
    Task<bool> IsInventoryNumberTakenAsync(string inventoryNumber);
    Task<(bool Success, string ErrorMessage)> AddAuthorAsync(string fullName);
}