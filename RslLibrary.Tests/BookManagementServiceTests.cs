using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RslLibrary.Services;
using RslLibrary.Models;
using RslLibrary.Data;
using RslLibrary.Constants;
using FluentValidation;

namespace RslLibrary.Tests;

public class BookManagementServiceTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task AddBookAsync_ShouldReturnTrue_WhenDataIsValid()
    {
        using var context = CreateInMemoryContext();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<BookManagementService>>();
        var mockValidator = new Mock<IValidator<CreateBookDto>>();

        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateBookDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        context.Authors.Add(new Author { Id = 1, FullName = "Тестовый Автор" });
        context.Categories.Add(new Category { Id = 1, Name = "Тестовая Категория" });
        await context.SaveChangesAsync();

        var service = new BookManagementService(
            context,
            mockAuditService.Object,
            mockLogger.Object,
            mockValidator.Object);

        var dto = new CreateBookDto
        {
            Title = "Тестовая книга",
            AuthorFullName = "Тестовый Автор",
            CategoryName = "Тестовая Категория",
            Year = 2024,
            PageCount = 100
        };

        var (success, errorMessage) = await service.AddBookAsync(dto, "INV-001", null, null);

        Assert.True(success);
        Assert.Empty(errorMessage);
        Assert.Equal(1, await context.Books.CountAsync());
        Assert.Equal(1, await context.Copies.CountAsync());
    }

    [Fact]
    public async Task AddBookAsync_ShouldReturnFalse_WhenInventoryNumberExists()
    {
        using var context = CreateInMemoryContext();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<BookManagementService>>();
        var mockValidator = new Mock<IValidator<CreateBookDto>>();

        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateBookDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        context.Copies.Add(new BookCopy { InventoryNumber = "INV-001" });
        await context.SaveChangesAsync();

        var service = new BookManagementService(
            context,
            mockAuditService.Object,
            mockLogger.Object,
            mockValidator.Object);

        var dto = new CreateBookDto
        {
            Title = "Тестовая книга",
            AuthorFullName = "Автор",
            CategoryName = "Категория"
        };

        var (success, errorMessage) = await service.AddBookAsync(dto, "INV-001", null, null);

        Assert.False(success);
        Assert.Contains("уже используется", errorMessage);
    }

    [Fact]
    public async Task DeleteBookAsync_ShouldRemoveBookAndCopies()
    {
        using var context = CreateInMemoryContext();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<BookManagementService>>();
        var mockValidator = new Mock<IValidator<CreateBookDto>>();

        var book = new Book
        {
            Id = 1,
            Title = "Книга для удаления",
            AuthorId = 1,
            CategoryId = 1,
            PublisherId = 1
        };

        context.Authors.Add(new Author { Id = 1, FullName = "Автор" });
        context.Categories.Add(new Category { Id = 1, Name = "Категория" });
        context.Publishers.Add(new Publisher { Id = 1, Name = "Издательство" });
        context.Books.Add(book);
        context.Copies.Add(new BookCopy { BookId = 1, InventoryNumber = "INV-001" });
        context.Copies.Add(new BookCopy { BookId = 1, InventoryNumber = "INV-002" });
        await context.SaveChangesAsync();

        var service = new BookManagementService(
            context,
            mockAuditService.Object,
            mockLogger.Object,
            mockValidator.Object);

        var result = await service.DeleteBookAsync(1);

        Assert.True(result);
        Assert.Equal(0, await context.Books.CountAsync());
        Assert.Equal(0, await context.Copies.CountAsync());
    }
}