namespace RslLibrary.Models;

public class ReservationStatus
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}