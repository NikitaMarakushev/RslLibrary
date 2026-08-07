using FluentValidation;
using RslLibrary.Services;

namespace RslLibrary.Validators;

public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название книги обязательно для заполнения.")
            .MaximumLength(200).WithMessage("Название книги не должно превышать 200 символов.");

        RuleFor(x => x.Isbn)
            .Matches(@"^(\d{13}|\d{10}|\d{3}-\d{1,5}-\d{1,7}-\d{1,7}-\d{1,3})$")
            .When(x => !string.IsNullOrWhiteSpace(x.Isbn))
            .WithMessage("Неверный формат ISBN. Допустимые форматы: 13 цифр или через дефис (978-5-...)");

        RuleFor(x => x.Year)
            .InclusiveBetween((short)1500, (short)DateTime.Now.Year)
            .When(x => x.Year.HasValue)
            .WithMessage($"Год издания должен быть между 1500 и {DateTime.Now.Year}.");

        RuleFor(x => x.PageCount)
            .GreaterThan(0).When(x => x.PageCount.HasValue)
            .WithMessage("Количество страниц должно быть больше 0.");

        RuleFor(x => x.AuthorFullName)
            .NotEmpty().WithMessage("Необходимо выбрать автора из списка.");

        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Необходимо выбрать категорию из списка.");
    }
}