namespace RslLibrary.Services;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public class ToastMessage
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ToastType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ToastService
{
    public event Action<ToastMessage>? OnToast;

    public void ShowSuccess(string title, string message)
    {
        Notify(title, message, ToastType.Success);
    }

    public void ShowError(string title, string message)
    {
        Notify(title, message, ToastType.Error);
    }

    public void ShowWarning(string title, string message)
    {
        Notify(title, message, ToastType.Warning);
    }

    public void ShowInfo(string title, string message)
    {
        Notify(title, message, ToastType.Info);
    }

    private void Notify(string title, string message, ToastType type)
    {
        OnToast?.Invoke(new ToastMessage
        {
            Title = title,
            Message = message,
            Type = type
        });
    }
}