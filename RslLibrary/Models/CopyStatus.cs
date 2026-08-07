namespace RslLibrary.Models;

public class CopyStatus
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
}