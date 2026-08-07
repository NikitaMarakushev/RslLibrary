namespace RslLibrary.Services;

public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public short? Year { get; set; }
    public string? Description { get; set; }
    public int? PageCount { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}