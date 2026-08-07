using RslLibrary.Constants;
using RslLibrary.Models;
using RslLibrary.Repositories;

namespace RslLibrary.Services;

public class CirculationService : ICirculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<CirculationService> _logger;
    private const int MaxAllowedBooks = 5;
    private const int StandardLoanDays = 14;

    public CirculationService(IUnitOfWork unitOfWork, IAuditService auditService, ILogger<CirculationService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<CirculationResult> IssueBookAsync(int userId, string inventoryNumber)
    {
        var copy = await _unitOfWork.Copies.GetByInventoryNumberAsync(inventoryNumber);
        if (copy == null)
            return CirculationResult.Failure("Экземпляр книги с таким инвентарным номером не найден.");

        if (copy.StatusId != CopyStatuses.Available)
            return CirculationResult.Failure($"Книга недоступна для выдачи. Текущий статус: {copy.StatusId}");

        var activeLoans = await _unitOfWork.Loans.GetActiveLoansCountByUserAsync(userId);
        if (activeLoans >= MaxAllowedBooks)
            return CirculationResult.Failure($"Превышен лимит книг. Читатель уже удерживает {activeLoans} экз.");

        var hasOverdue = await _unitOfWork.Loans.HasUserOverdueLoansAsync(userId);
        if (hasOverdue)
            return CirculationResult.Failure("Выдача заблокирована. У читателя имеются задолженности по срокам возврата.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var loan = new Loan
        {
            CopyId = copy.Id,
            UserId = userId,
            LoanDate = today,
            DueDate = today.AddDays(StandardLoanDays),
            StatusId = LoanStatuses.Active,
            CreatedAt = DateTime.UtcNow
        };

        var oldStatus = copy.StatusId;
        copy.StatusId = CopyStatuses.Loaned;
        await _unitOfWork.Loans.AddAsync(loan);
        await _unitOfWork.CompleteAsync();

        // Запись в аудит
        await _auditService.LogAsync(
            "ISSUE",
            "Loan",
            loan.Id,
            new { Status = "Available" },
            new { Status = "Loaned", UserId = userId, DueDate = loan.DueDate }
        );

        _logger.LogInformation($"Экземпляр {inventoryNumber} выдан пользователю ID: {userId}. Срок до: {loan.DueDate}");
        return CirculationResult.Success();
    }

    public async Task<CirculationResult> ReturnBookAsync(string inventoryNumber)
    {
        var copy = await _unitOfWork.Copies.GetByInventoryNumberAsync(inventoryNumber);
        if (copy == null || copy.StatusId != CopyStatuses.Loaned)
            return CirculationResult.Failure("Данный экземпляр не числится как выданный.");

        var activeLoan = await _unitOfWork.Loans.GetActiveLoanByCopyIdAsync(copy.Id);
        if (activeLoan == null)
            return CirculationResult.Failure("Не найдена активная запись о выдаче для данного экземпляра.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        activeLoan.ReturnDate = today;
        activeLoan.StatusId = LoanStatuses.Returned;
        copy.StatusId = CopyStatuses.Available;

        int? overdueDays = null;
        if (activeLoan.ReturnDate > activeLoan.DueDate)
        {
            overdueDays = activeLoan.ReturnDate.Value.DayNumber - activeLoan.DueDate.DayNumber;
            _logger.LogWarning($"Книга {inventoryNumber} возвращена с просрочкой в {overdueDays} дней.");
        }

        await _unitOfWork.CompleteAsync();

        await _auditService.LogAsync(
            "RETURN",
            "Loan",
            activeLoan.Id,
            new { Status = "Loaned" },
            new { Status = "Returned", ReturnDate = today, OverdueDays = overdueDays }
        );

        return CirculationResult.Success();
    }

    public async Task<CopyInfoDto?> GetCopyInfoAsync(string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(inventoryNumber)) return null;

        var copy = await _unitOfWork.Copies.GetByInventoryNumberAsync(inventoryNumber.Trim());

        if (copy == null) return null;

        return new CopyInfoDto
        {
            InventoryNumber = copy.InventoryNumber,
            BookTitle = copy.Book?.Title ?? "Название не указано",
            AuthorName = copy.Book?.Author?.FullName ?? "Автор не указан",
            IsAvailable = copy.StatusId == CopyStatuses.Available,
            StatusName = GetStatusName(copy.StatusId)
        };
    }
    private string GetStatusName(short statusId)
    {
        return statusId switch
        {
            1 => "Доступен",
            2 => "Выдан",
            3 => "Зарезервирован",
            _ => "Неизвестный статус"
        };
    }
}

public class CopyInfoDto
{
    public string InventoryNumber { get; set; } = "";
    public string BookTitle { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public bool IsAvailable { get; set; }
    public string StatusName { get; set; } = "";
}