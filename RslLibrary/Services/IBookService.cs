using RslLibrary.Models;

namespace RslLibrary.Services;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<BookDto?> GetBookByIdAsync(int id);
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<IEnumerable<Author>> GetAuthorsAsync();
}