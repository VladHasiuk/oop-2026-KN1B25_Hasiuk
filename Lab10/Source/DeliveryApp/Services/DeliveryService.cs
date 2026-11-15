using DeliveryApp.Models;

namespace DeliveryApp.Services;

/// <summary>
/// Межа оброблення помилок: тут очікувані винятки перетворюються на
/// зрозумілі результати. Неочікувані (помилки програмування) не перехоплюються.
/// </summary>
public class DeliveryService
{
    private readonly DeliveryCenter _center;

    public DeliveryService(DeliveryCenter center)
    {
        _center = center ?? throw new ArgumentNullException(nameof(center));
    }

    /// <summary>Ознака того, що операція виконується. Скидається у finally.</summary>
    public bool IsProcessing { get; private set; }

    /// <summary>
    /// Реєструє стандартну посилку. Некоректні дані відхиляються конструктором
    /// до реєстрації, тому колекція ніколи не містить частково створених об'єктів.
    /// </summary>
    public OperationResult<Parcel> RegisterStandard(
        string number, double weight, string recipient, string city, string? address = null)
    {
        IsProcessing = true;
        try
        {
            StandardParcel parcel = new(number, weight, recipient, city) { Address = address };
            return _center.Register(parcel);
        }
        catch (ArgumentOutOfRangeException ex)   // раніше за ArgumentException: він його нащадок
        {
            return OperationResult<Parcel>.Failure(
                $"Відмова: поле «{ex.ParamName}» поза допустимим діапазоном. {Clean(ex.Message)} (отримано: {ex.ActualValue})");
        }
        catch (ArgumentException ex)
        {
            return OperationResult<Parcel>.Failure(
                $"Відмова: некоректне поле «{ex.ParamName}». {Clean(ex.Message)}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>Переводить посилку на новий етап доставки.</summary>
    public OperationResult<Parcel> UpdateStage(string number, ParcelStatus newStatus)
    {
        // Порушення контракту виклику: помилка програмування, сервіс її не приховує
        ArgumentNullException.ThrowIfNull(number);

        Parcel? parcel = null;
        IsProcessing = true;
        try
        {
            parcel = _center.FindByNumber(number);
            if (parcel is null)
            {
                // Очікуваний результат, а не виняток
                return OperationResult<Parcel>.Failure($"Посилку {number} не знайдено");
            }

            ParcelStatus previous = parcel.Status;
            parcel.ChangeStatus(newStatus);
            return OperationResult<Parcel>.Success(
                parcel, $"Посилка {number}: {Parcel.Describe(previous)} → {Parcel.Describe(newStatus)}");
        }
        catch (ParcelAlreadyDeliveredException ex)   // конкретніший тип іде першим
        {
            return OperationResult<Parcel>.Failure(
                $"Відмова: {ex.Message} Стан не змінено: «{Parcel.Describe(parcel!.Status)}».");
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<Parcel>.Failure($"Відмова: {ex.Message}");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return OperationResult<Parcel>.Failure($"Відмова: {Clean(ex.Message)} (отримано: {ex.ActualValue})");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>Прибирає з повідомлення виняткової ситуації технічний суфікс та зайві рядки.</summary>
    private static string Clean(string message)
    {
        string firstLine = message.Split('\n')[0].TrimEnd('\r');
        int marker = firstLine.IndexOf(" (Parameter '", StringComparison.Ordinal);
        return marker >= 0 ? firstLine[..marker] : firstLine;
    }
}
