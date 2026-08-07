namespace RslLibrary.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public short? PublicationYear { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
    public int PublisherId { get; set; }
    public Publisher Publisher { get; set; } = null!;
    public string? Language { get; set; } 
    public int? PageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public short? LanguageId { get; set; }
    public Language? LanguageRef { get; set; }

    public bool IsDeleted { get; set; } = false;
    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
}