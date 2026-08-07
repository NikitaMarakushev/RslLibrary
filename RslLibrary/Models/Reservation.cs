using RslLibrary.Data;
namespace RslLibrary.Models;

public class Reservation
{
    public int Id { get; set; }
    public int CopyId { get; set; }
    public BookCopy Copy { get; set; } = null!;
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateOnly ReservationDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public short StatusId { get; set; }
    public ReservationStatus Status { get; set; } = null!;
}