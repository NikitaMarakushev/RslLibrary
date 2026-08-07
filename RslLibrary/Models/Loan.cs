using RslLibrary.Data;
namespace RslLibrary.Models;

public class Loan
{
    public int Id { get; set; }
    public int CopyId { get; set; }
    public BookCopy Copy { get; set; } = null!;
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateOnly LoanDate { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public short StatusId { get; set; }
    public LoanStatus Status { get; set; } = null!;
}