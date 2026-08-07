namespace RslLibrary.Models;

public class Author
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public short? BirthYear { get; set; }
    public short? DeathYear { get; set; }
    public string? Country { get; set; }
    public string? Biography { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Book> Books { get; set; } = new List<Book>();
}