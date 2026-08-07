using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RslLibrary.Constants;
using RslLibrary.Models;

namespace RslLibrary.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        Console.WriteLine("🚀 Начинаем инициализацию базы данных...");

        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("✅ Миграции применены");

            await SeedRolesAsync(roleManager);
            Console.WriteLine("✅ Роли созданы");

            await SeedReferenceDataAsync(context);
            Console.WriteLine("✅ Справочники созданы");

            await SeedUsersAsync(userManager, context);
            Console.WriteLine("✅ Пользователи созданы");

            await SeedBooksDataAsync(context);
            Console.WriteLine("✅ Книги созданы");

            Console.WriteLine("🎉 Инициализация базы данных завершена успешно!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ОШИБКА при инициализации БД: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roleNames = { RoleNames.Administrator, RoleNames.Librarian, RoleNames.Reader };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    Description = roleName switch
                    {
                        RoleNames.Administrator => "Полный доступ к системе",
                        RoleNames.Librarian => "Работа с библиотечным фондом",
                        RoleNames.Reader => "Читатель библиотеки",
                        _ => ""
                    },
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private static async Task SeedReferenceDataAsync(ApplicationDbContext context)
    {
        if (!await context.CopyStatuses.AnyAsync())
        {
            await context.CopyStatuses.AddRangeAsync(
                new CopyStatus { Id = 1, Name = "Available", Description = "Экземпляр доступен" },
                new CopyStatus { Id = 2, Name = "Loaned", Description = "Выдан читателю" },
                new CopyStatus { Id = 3, Name = "Reserved", Description = "Зарезервирован" },
                new CopyStatus { Id = 4, Name = "Lost", Description = "Утерян" },
                new CopyStatus { Id = 5, Name = "Repair", Description = "На реставрации" }
            );
        }

        if (!await context.LoanStatuses.AnyAsync())
        {
            await context.LoanStatuses.AddRangeAsync(
                new LoanStatus { Id = 1, Name = "Active", Description = "Книга выдана" },
                new LoanStatus { Id = 2, Name = "Returned", Description = "Возвращена" },
                new LoanStatus { Id = 3, Name = "Overdue", Description = "Просрочена" }
            );
        }

        if (!await context.ReservationStatuses.AnyAsync())
        {
            await context.ReservationStatuses.AddRangeAsync(
                new ReservationStatus { Id = 1, Name = "Waiting", Description = "Ожидает" },
                new ReservationStatus { Id = 2, Name = "Ready", Description = "Готово к выдаче" },
                new ReservationStatus { Id = 3, Name = "Completed", Description = "Получено" },
                new ReservationStatus { Id = 4, Name = "Cancelled", Description = "Отменено" }
            );
        }

        if (!await context.Categories.AnyAsync())
        {
            await context.Categories.AddRangeAsync(
                new Category { Id = 1, Name = "Программирование", Description = "Книги по программированию" },
                new Category { Id = 2, Name = "Информационные технологии", Description = "ИТ литература" },
                new Category { Id = 3, Name = "Математика", Description = "Научная литература" },
                new Category { Id = 4, Name = "История", Description = "Историческая литература" },
                new Category { Id = 5, Name = "Художественная литература", Description = "Художественные произведения" }
            );
        }

        await context.SaveChangesAsync();

        await context.Database.ExecuteSqlRawAsync(@"
            SELECT setval('""copy_statuses_Id_seq""', (SELECT MAX(""Id"") FROM copy_statuses));
            SELECT setval('""loan_statuses_Id_seq""', (SELECT MAX(""Id"") FROM loan_statuses));
            SELECT setval('""reservation_statuses_Id_seq""', (SELECT MAX(""Id"") FROM reservation_statuses));
            SELECT setval('""categories_Id_seq""', (SELECT MAX(""Id"") FROM categories));
        ");
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        Console.WriteLine("👤 Начинаем создание пользователей...");

        // Администратор
        var adminEmail = "admin@library.local";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "Администратор системы",
                ReaderTicketNumber = "000001",
                PhoneNumber = "79990000001",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, RoleNames.Administrator);
                Console.WriteLine($"✅ Создан администратор: {adminEmail}");
            }
            else
            {
                Console.WriteLine($"❌ Ошибка создания администратора: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"⚠️ Администратор {adminEmail} уже существует");
        }

        // Библиотекарь
        var librarianEmail = "librarian@library.local";
        if (await userManager.FindByEmailAsync(librarianEmail) == null)
        {
            var librarian = new ApplicationUser
            {
                UserName = "librarian",
                Email = librarianEmail,
                FullName = "Мария Петрова",
                ReaderTicketNumber = "000002",
                PhoneNumber = "79990000002",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(librarian, "Lib123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(librarian, RoleNames.Librarian);
                Console.WriteLine($"✅ Создан библиотекарь: {librarianEmail}");
            }
            else
            {
                Console.WriteLine($"❌ Ошибка создания библиотекаря: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"⚠️ Библиотекарь {librarianEmail} уже существует");
        }

        // Читатели
        var readers = new[]
        {
        ("reader1@mail.ru", "reader1", "Иван Иванов", "100001", "79990000003"),
        ("reader2@mail.ru", "reader2", "Анна Смирнова", "100002", "79990000004"),
        ("reader3@mail.ru", "reader3", "Павел Соколов", "100003", "79990000005")
    };

        foreach (var (email, userName, fullName, ticket, phone) in readers)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var reader = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    FullName = fullName,
                    ReaderTicketNumber = ticket,
                    PhoneNumber = phone,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(reader, "Reader123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(reader, RoleNames.Reader);
                    Console.WriteLine($"✅ Создан читатель: {email}");
                }
                else
                {
                    Console.WriteLine($"❌ Ошибка создания читателя {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Читатель {email} уже существует");
            }
        }
    }

    private static async Task SeedBooksDataAsync(ApplicationDbContext context)
    {
        if (!await context.Authors.AnyAsync())
        {
            await context.Authors.AddRangeAsync(
                new Author { Id = 1, FullName = "Роберт Мартин", BirthYear = 1952, Country = "США", Biography = "Автор книг по разработке ПО", CreatedAt = DateTime.UtcNow },
                new Author { Id = 2, FullName = "Мартин Фаулер", BirthYear = 1963, Country = "Великобритания", Biography = "Эксперт в области архитектуры ПО", CreatedAt = DateTime.UtcNow },
                new Author { Id = 3, FullName = "Эндрю Троелсен", BirthYear = 1970, Country = "США", Biography = "Автор книг по C#", CreatedAt = DateTime.UtcNow },
                new Author { Id = 4, FullName = "Кристофер Дейт", BirthYear = 1941, Country = "Великобритания", Biography = "Специалист по базам данных", CreatedAt = DateTime.UtcNow },
                new Author { Id = 5, FullName = "Лев Толстой", BirthYear = 1828, DeathYear = 1910, Country = "Россия", Biography = "Русский писатель", CreatedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            await context.Database.ExecuteSqlRawAsync(@"
            SELECT setval('""authors_Id_seq""', (SELECT MAX(""Id"") FROM authors));
        ");
        }

        if (!await context.Publishers.AnyAsync())
        {
            await context.Publishers.AddRangeAsync(
                new Publisher { Id = 1, Name = "Питер", City = "Санкт-Петербург", Website = "https://piter.com", CreatedAt = DateTime.UtcNow },
                new Publisher { Id = 2, Name = "Диалектика", City = "Киев", Website = "https://dialektika.com", CreatedAt = DateTime.UtcNow },
                new Publisher { Id = 3, Name = "Вильямс", City = "Москва", Website = "https://williamspublishing.com", CreatedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            await context.Database.ExecuteSqlRawAsync(@"
            SELECT setval('""publishers_Id_seq""', (SELECT MAX(""Id"") FROM publishers));
        ");
        }

        if (!await context.Books.AnyAsync())
        {
            var books = new List<Book>
        {
            new Book { Title = "Чистая архитектура", Isbn = "9785446116186", PublicationYear = 2022, Description = "Архитектура программного обеспечения", CategoryId = 1, AuthorId = 1, PublisherId = 1, Language = "Русский", PageCount = 352, CreatedAt = DateTime.UtcNow },
            new Book { Title = "Архитектура корпоративных приложений", Isbn = "9785845905796", PublicationYear = 2019, Description = "Корпоративные приложения", CategoryId = 2, AuthorId = 2, PublisherId = 3, Language = "Русский", PageCount = 544, CreatedAt = DateTime.UtcNow },
            new Book { Title = "Язык программирования C#", Isbn = "9786010842726", PublicationYear = 2023, Description = "C# и .NET", CategoryId = 1, AuthorId = 3, PublisherId = 2, Language = "Русский", PageCount = 1100, CreatedAt = DateTime.UtcNow },
            new Book { Title = "Введение в системы баз данных", Isbn = "9785845920850", PublicationYear = 2021, Description = "Реляционные базы данных", CategoryId = 2, AuthorId = 4, PublisherId = 3, Language = "Русский", PageCount = 1328, CreatedAt = DateTime.UtcNow },
            new Book { Title = "Война и мир", Isbn = "9785170908767", PublicationYear = 2018, Description = "Роман", CategoryId = 5, AuthorId = 5, PublisherId = 1, Language = "Русский", PageCount = 1225, CreatedAt = DateTime.UtcNow }
        };

            context.Books.AddRange(books);
            await context.SaveChangesAsync();

            await context.Database.ExecuteSqlRawAsync(@"
            SELECT setval('""books_Id_seq""', (SELECT MAX(""Id"") FROM books));
        ");

            var copies = new List<BookCopy>();
            for (int i = 0; i < books.Count; i++)
            {
                copies.Add(new BookCopy
                {
                    BookId = books[i].Id,
                    InventoryNumber = $"RBG-{(i + 1):D6}",
                    Barcode = $"46000000000{i + 1}",
                    Location = $"Фонд А-{100 + i}",
                    AcquisitionDate = DateOnly.FromDateTime(DateTime.Today),
                    Condition = "Good",
                    StatusId = CopyStatuses.Available,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.Copies.AddRange(copies);
            await context.SaveChangesAsync();

            await context.Database.ExecuteSqlRawAsync(@"
            SELECT setval('""copies_Id_seq""', (SELECT MAX(""Id"") FROM copies));
        ");
        }
    }
}