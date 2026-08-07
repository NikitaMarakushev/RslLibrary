namespace RslLibrary.Models;

public class LoanStatus
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}