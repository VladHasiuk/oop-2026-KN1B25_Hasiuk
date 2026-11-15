namespace DeliveryApp.Models;

/// <summary>
/// Узагальнений контейнер для відправлень. Обмеження where T : Parcel
/// дозволяє зберігати будь-який вид посилки та користуватись її властивостями.
/// </summary>
public class Repository<T> where T : Parcel
{
    private readonly List<T> _items = new();

    public int Count => _items.Count;

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    /// <summary>Видаляє елемент. Повертає false, якщо його не було.</summary>
    public bool Remove(T item) => _items.Remove(item);

    /// <summary>Усі елементи лише для читання: зовні список змінити не можна.</summary>
    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

    /// <summary>
    /// Ітератор: по одному повертає відправлення із заданим статусом,
    /// не створюючи проміжної колекції.
    /// </summary>
    public IEnumerable<T> GetByStatus(ParcelStatus status)
    {
        foreach (T item in _items)
        {
            if (item.Status == status)
            {
                yield return item;
            }
        }
    }
}
