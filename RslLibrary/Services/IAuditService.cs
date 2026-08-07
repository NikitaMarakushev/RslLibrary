namespace RslLibrary.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, int? entityId, object? oldValues = null, object? newValues = null);
}