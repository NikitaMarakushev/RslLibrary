using RslLibrary.Models;

namespace RslLibrary.Services;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public short? Year { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string AuthorFullName { get; set; } = string.Empty;
    public int AvailableCopiesCount { get; set; }
    public List<string> InventoryNumbers { get; set; } = new();
}

public interface IBookSpecificationService
{
    Task<PagedList<BookDto>> GetFilteredBooksAsync(
        string? searchTerm,
        int? categoryId,
        int? authorId,
        int pageIndex,
        int pageSize);
}