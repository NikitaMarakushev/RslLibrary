namespace RslLibrary.Models;

public class BookCopy
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public string InventoryNumber { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Location { get; set; }
    public DateOnly? AcquisitionDate { get; set; }
    public string? Condition { get; set; }
    public DateTime CreatedAt { get; set; }
    public short StatusId { get; set; }
    public CopyStatus Status { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}