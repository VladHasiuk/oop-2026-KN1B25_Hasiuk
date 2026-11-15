namespace DeliveryApp.Models;

/// <summary>
/// Результат однієї операції: ознака успіху, значення типу T і повідомлення.
/// </summary>
public sealed class OperationResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Message { get; }

    // Закритий конструктор: створити результат можна лише через Success/Failure
    private OperationResult(bool isSuccess, T? value, string message)
    {
        IsSuccess = isSuccess;
        Value = value;
        Message = message;
    }

    public static OperationResult<T> Success(T value, string message = "Операцію виконано")
        => new(true, value, message);

    public static OperationResult<T> Failure(string message)
        => new(false, default, message);
}
