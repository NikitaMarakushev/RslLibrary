using Xunit;
using RslLibrary.Validators;
using RslLibrary.Services;

namespace RslLibrary.Tests;

public class CreateBookDtoValidatorTests
{
    private readonly CreateBookDtoValidator _validator;

    public CreateBookDtoValidatorTests()
    {
        _validator = new CreateBookDtoValidator();
    }

    [Fact]
    public void Validator_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var dto = new CreateBookDto
        {
            Title = "Чистая архитектура",
            Isbn = "9785446116186",
            Year = 2022,
            PageCount = 352,
            AuthorFullName = "Роберт Мартин",
            CategoryName = "Программирование"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var dto = new CreateBookDto
        {
            Title = "",
            AuthorFullName = "Автор",
            CategoryName = "Категория"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validator_ShouldFail_WhenYearIsInvalid()
    {
        // Arrange
        var dto = new CreateBookDto
        {
            Title = "Книга",
            Year = 3000, // Будущий год
            AuthorFullName = "Автор",
            CategoryName = "Категория"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Year");
    }

    [Fact]
    public void Validator_ShouldFail_WhenIsbnFormatIsInvalid()
    {
        // Arrange
        var dto = new CreateBookDto
        {
            Title = "Книга",
            Isbn = "invalid-isbn",
            AuthorFullName = "Автор",
            CategoryName = "Категория"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Isbn");
    }
}