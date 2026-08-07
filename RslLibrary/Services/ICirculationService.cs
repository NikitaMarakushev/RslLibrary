namespace RslLibrary.Services;

public class CirculationResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static CirculationResult Success() => new() { IsSuccess = true };
    public static CirculationResult Failure(string message) => new() { IsSuccess = false, Message = message };
}

public interface ICirculationService
{
    Task<CirculationResult> IssueBookAsync(int userId, string inventoryNumber);
    Task<CirculationResult> ReturnBookAsync(string inventoryNumber);
    Task<CopyInfoDto?> GetCopyInfoAsync(string inventoryNumber);
}