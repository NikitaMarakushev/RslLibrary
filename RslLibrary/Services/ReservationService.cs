using RslLibrary.Constants;
using RslLibrary.Models;
using RslLibrary.Repositories;

namespace RslLibrary.Services;

public class ReservationService : IReservationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(IUnitOfWork unitOfWork, IAuditService auditService, ILogger<ReservationService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<bool> CreateReservationAsync(int userId, int copyId)
    {
        var copy = await _unitOfWork.Copies.GetByIdAsync(copyId);
        if (copy == null || copy.StatusId != CopyStatuses.Available)
            return false;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var reservation = new Reservation
        {
            CopyId = copyId,
            UserId = userId,
            ReservationDate = today,
            ExpirationDate = today.AddDays(3),
            StatusId = ReservationStatuses.Waiting,
            CreatedAt = DateTime.UtcNow
        };

        copy.StatusId = CopyStatuses.Reserved;
        await _unitOfWork.Reservations.AddAsync(reservation);
        await _unitOfWork.CompleteAsync();

        // Запись в аудит
        await _auditService.LogAsync(
            "RESERVE",
            "Reservation",
            reservation.Id,
            null,
            new { CopyId = copyId, UserId = userId, ExpirationDate = reservation.ExpirationDate }
        );

        _logger.LogInformation($"Пользователь {userId} забронировал экземпляр {copyId}");
        return true;
    }

    public async Task<bool> CancelReservationAsync(int reservationId, int userId)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId);
        if (reservation == null || reservation.UserId != userId)
            return false;

        var oldStatus = reservation.StatusId;
        reservation.StatusId = ReservationStatuses.Cancelled;
        var copy = await _unitOfWork.Copies.GetByIdAsync(reservation.CopyId);
        if (copy != null && copy.StatusId == CopyStatuses.Reserved)
            copy.StatusId = CopyStatuses.Available;

        await _unitOfWork.CompleteAsync();

        // Запись в аудит
        await _auditService.LogAsync(
            "CANCEL_RESERVATION",
            "Reservation",
            reservation.Id,
            new { Status = oldStatus },
            new { Status = ReservationStatuses.Cancelled }
        );

        return true;
    }
}