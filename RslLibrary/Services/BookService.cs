using Microsoft.EntityFrameworkCore;
using RslLibrary.Constants;
using RslLibrary.Data;
using RslLibrary.Models;

namespace RslLibrary.Services;

public class BookService : IBookService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public BookService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .AsNoTracking()
            .OrderBy(b => b.Title)
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
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Include(b => b.Publisher)
            .AsNoTracking()
            .Where(b => b.Id == id)
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
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IEnumerable<Author>> GetAuthorsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Authors.OrderBy(a => a.FullName).ToListAsync();
    }
}