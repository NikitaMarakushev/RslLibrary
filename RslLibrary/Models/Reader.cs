namespace RslLibrary.Models;

public class Reader
{
    public int Id { get; set; }

    public string LastName { get; set; } = "";

    public string FirstName { get; set; } = "";

    public string? MiddleName { get; set; }

    public string Phone { get; set; } = "";

    public string Email { get; set; } = "";

    public DateTime RegistrationDate { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}