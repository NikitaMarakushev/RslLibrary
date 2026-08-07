using Microsoft.EntityFrameworkCore;
using RslLibrary.Constants;
using RslLibrary.Data;
using RslLibrary.Models;

namespace RslLibrary.Services;

public class BookSpecificationService : IBookSpecificationService
{
    private readonly ApplicationDbContext _context;

    public BookSpecificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<BookDto>> GetFilteredBooksAsync(
        string? searchTerm,
        int? categoryId,
        int? authorId,
        int pageIndex,
        int pageSize)
    {
        IQueryable<Book> query = _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(searchTerm) ||
                (b.Isbn != null && b.Isbn.Contains(searchTerm)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == categoryId.Value);
        }

        if (authorId.HasValue)
        {
            query = query.Where(b => b.AuthorId == authorId.Value);
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(b => b.Title)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Isbn = b.Isbn,
                Year = b.PublicationYear,
                CategoryName = b.Category.Name,
                AuthorFullName = b.Author.FullName,
                AvailableCopiesCount = b.Copies.Count(c => c.StatusId == CopyStatuses.Available)
            })
            .ToListAsync();

        return new PagedList<BookDto>(items, totalCount, pageIndex, pageSize);
    }
}