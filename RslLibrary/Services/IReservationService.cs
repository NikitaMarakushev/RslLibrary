namespace RslLibrary.Services;

public interface IReservationService
{
    Task<bool> CreateReservationAsync(int userId, int copyId);
    Task<bool> CancelReservationAsync(int reservationId, int userId);
}