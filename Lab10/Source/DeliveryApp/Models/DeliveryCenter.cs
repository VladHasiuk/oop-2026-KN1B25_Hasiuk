using DeliveryApp.Helpers;

namespace DeliveryApp.Models;

/// <summary>
/// Центр доставки. Поєднує три колекції (композиція):
/// Repository — усі відправлення, Dictionary — швидкий пошук за номером,
/// Queue — черга на доставку (FIFO: першим доставляється те, що стало першим).
/// </summary>
public class DeliveryCenter
{
    private readonly Repository<Parcel> _parcels = new();
    private readonly Dictionary<string, Parcel> _byNumber = new();
    private readonly Queue<Parcel> _queue = new();

    public int Count => _parcels.Count;
    public int QueueLength => _queue.Count;

    public IReadOnlyList<Parcel> GetAll() => _parcels.GetAll();

    /// <summary>Реєструє відправлення. Номер має бути унікальним.</summary>
    public OperationResult<Parcel> Register(Parcel parcel)
    {
        ArgumentNullException.ThrowIfNull(parcel);

        // TryAdd повертає false, якщо такий ключ уже є
        if (!_byNumber.TryAdd(parcel.Number, parcel))
        {
            return OperationResult<Parcel>.Failure($"Посилка {parcel.Number} уже зареєстрована");
        }

        _parcels.Add(parcel);
        return OperationResult<Parcel>.Success(parcel, $"Посилку {parcel.Number} зареєстровано");
    }

    /// <summary>Пошук за номером через словник. Якщо не знайдено — null.</summary>
    public Parcel? FindByNumber(string number)
        => _byNumber.GetValueOrDefault(number);

    /// <summary>Ставить відправлення в кінець черги на доставку.</summary>
    public OperationResult<Parcel> EnqueueForDelivery(string number)
    {
        Parcel? parcel = FindByNumber(number);
        if (parcel is null)
        {
            return OperationResult<Parcel>.Failure($"Посилку {number} не знайдено");
        }

        if (parcel.Status != ParcelStatus.Created)
        {
            return OperationResult<Parcel>.Failure($"Посилка {number} уже не потребує постановки в чергу");
        }

        if (_queue.Contains(parcel))
        {
            return OperationResult<Parcel>.Failure($"Посилка {number} уже в черзі");
        }

        _queue.Enqueue(parcel);
        return OperationResult<Parcel>.Success(parcel, $"Посилку {number} додано в чергу");
    }

    /// <summary>Бере з початку черги наступне відправлення та доставляє його кур'єром.</summary>
    public OperationResult<Parcel> DeliverNext(Courier courier)
    {
        if (_queue.Count == 0)
        {
            return OperationResult<Parcel>.Failure("Черга доставки порожня");
        }

        Parcel next = _queue.Dequeue();
        return ParcelHelper.TryDeliver(courier, next);
    }

    /// <summary>Видаляє відправлення, якщо воно не очікує в черзі.</summary>
    public OperationResult<Parcel> Remove(string number)
    {
        Parcel? parcel = FindByNumber(number);
        if (parcel is null)
        {
            return OperationResult<Parcel>.Failure($"Посилку {number} не знайдено");
        }

        if (_queue.Contains(parcel))
        {
            return OperationResult<Parcel>.Failure($"Посилку {number} не можна видалити: вона в черзі");
        }

        _parcels.Remove(parcel);
        _byNumber.Remove(number);
        return OperationResult<Parcel>.Success(parcel, $"Посилку {number} видалено");
    }

    /// <summary>Відправлення із заданим статусом (ітератор з yield return).</summary>
    public IEnumerable<Parcel> GetByStatus(ParcelStatus status)
        => _parcels.GetByStatus(status);

    /// <summary>Поточна черга в порядку доставки. Сама черга ззовні недоступна.</summary>
    public IEnumerable<Parcel> GetQueue()
    {
        foreach (Parcel parcel in _queue)
        {
            yield return parcel;
        }
    }
}
