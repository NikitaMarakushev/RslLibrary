using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RslLibrary.Constants;
using RslLibrary.Data;
using RslLibrary.Models;

namespace RslLibrary.Services;

public class BookManagementService : IBookManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<BookManagementService> _logger;
    private readonly IValidator<CreateBookDto> _bookValidator;

    public BookManagementService(
        ApplicationDbContext context,
        IAuditService auditService,
        ILogger<BookManagementService> logger,
        IValidator<CreateBookDto> bookValidator)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
        _bookValidator = bookValidator;
    }

    public async Task<(bool Success, string ErrorMessage)> AddBookAsync(CreateBookDto bookDto, string inventoryNumber, string? barcode, string? location)
    {
        try
        {
            var validationResult = await _bookValidator.ValidateAsync(bookDto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning($"Ошибка валидации при добавлении книги: {errors}");
                return (false, errors);
            }

            var existingCopy = await _context.Copies
                .FirstOrDefaultAsync(c => c.InventoryNumber == inventoryNumber);

            if (existingCopy != null)
            {
                _logger.LogWarning($"Инвентарный номер '{inventoryNumber}' уже используется");
                return (false, $"Инвентарный номер '{inventoryNumber}' уже используется другой книгой.");
            }

            var author = await _context.Authors.FirstOrDefaultAsync(a => a.FullName == bookDto.AuthorFullName);
            if (author == null)
            {
                _logger.LogWarning($"Автор '{bookDto.AuthorFullName}' не найден");
                return (false, $"Автор '{bookDto.AuthorFullName}' не найден в базе данных.");
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == bookDto.CategoryName);
            if (category == null)
            {
                _logger.LogWarning($"Категория '{bookDto.CategoryName}' не найдена");
                return (false, $"Категория '{bookDto.CategoryName}' не найдена в базе данных.");
            }

            var book = new Book
            {
                Title = bookDto.Title,
                Isbn = bookDto.Isbn,
                PublicationYear = bookDto.Year,
                Description = bookDto.Description,
                AuthorId = author.Id,
                CategoryId = category.Id,
                PublisherId = 1,
                Language = "Русский",
                PageCount = bookDto.PageCount,
                CreatedAt = DateTime.UtcNow
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var copy = new BookCopy
            {
                BookId = book.Id,
                InventoryNumber = inventoryNumber,
                Barcode = barcode,
                Location = location,
                AcquisitionDate = DateOnly.FromDateTime(DateTime.Today),
                Condition = "New",
                StatusId = CopyStatuses.Available,
                CreatedAt = DateTime.UtcNow
            };

            _context.Copies.Add(copy);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                "CREATE",
                "Book",
                book.Id,
                null,
                new { book.Title, book.Isbn, InventoryNumber = inventoryNumber }
            );

            _logger.LogInformation($"Добавлена книга '{book.Title}' с инвентарным номером {inventoryNumber}");
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении книги");
            return (false, $"Произошла ошибка: {ex.Message}");
        }
    }

    public async Task<bool> UpdateBookAsync(int bookId, CreateBookDto bookDto)
    {
        try
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return false;

            var oldValues = new { book.Title, book.Isbn, book.PublicationYear };

            book.Title = bookDto.Title;
            book.Isbn = bookDto.Isbn;
            book.PublicationYear = bookDto.Year;
            book.Description = bookDto.Description;
            book.PageCount = bookDto.PageCount;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "Book", bookId, oldValues,
                new { bookDto.Title, bookDto.Isbn, bookDto.Year });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении книги");
            return false;
        }
    }

    public async Task<bool> DeleteBookAsync(int bookId)
    {
        try
        {
            var book = await _context.Books
                .Include(b => b.Copies)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null) return false;

            book.IsDeleted = true;
            book.UpdatedAt = DateTime.UtcNow;

            foreach (var copy in book.Copies)
            {
                copy.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("DELETE", "Book", bookId,
                new { book.Title, CopiesCount = book.Copies.Count }, null);

            _logger.LogInformation($"Книга '{book.Title}' (Id: {bookId}) успешно помечена как удаленная (Soft Delete).");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении книги");
            return false;
        }
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Include(b => b.Copies)
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
                AvailableCopiesCount = b.Copies.Count(c => c.StatusId == CopyStatuses.Available),
                InventoryNumbers = b.Copies.Select(c => c.InventoryNumber).ToList()
            })
            .ToListAsync();
    }

    public async Task<CreateBookDto?> GetBookForEditAsync(int bookId)
    {
        var book = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book == null) return null;

        return new CreateBookDto
        {
            Title = book.Title,
            Isbn = book.Isbn,
            Year = book.PublicationYear,
            Description = book.Description,
            PageCount = book.PageCount,
            AuthorFullName = book.Author.FullName,
            CategoryName = book.Category.Name
        };
    }

    public async Task<bool> IsInventoryNumberTakenAsync(string inventoryNumber)
    {
        return await _context.Copies.AnyAsync(c => c.InventoryNumber == inventoryNumber);
    }

    public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
    {
        return await _context.Authors.OrderBy(a => a.FullName).ToListAsync();
    }

    public async Task<IEnumerable<Publisher>> GetAllPublishersAsync()
    {
        return await _context.Publishers.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<(bool Success, string ErrorMessage)> AddAuthorAsync(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (false, "Имя автора не может быть пустым.");
        }

        var normalizedFullName = fullName.Trim();

        var exists = await _context.Authors.AnyAsync(a => a.FullName.ToLower() == normalizedFullName.ToLower());
        if (exists)
        {
            return (false, "Автор с таким именем уже существует в базе данных.");
        }

        var newAuthor = new Author
        {
            FullName = normalizedFullName
        };

        _context.Authors.Add(newAuthor);
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}