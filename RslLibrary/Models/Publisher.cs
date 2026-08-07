namespace RslLibrary.Models;

public class Publisher
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Website { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Book> Books { get; set; } = new List<Book>();
}