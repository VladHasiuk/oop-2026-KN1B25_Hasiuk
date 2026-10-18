namespace DeliveryApp.Models;

/// <summary>
/// Узагальнене сховище об'єктів. T може бути будь-яким типом:
/// Repository&lt;Parcel&gt;, Repository&lt;FragileParcel&gt;, Repository&lt;Courier&gt; тощо.
/// </summary>
public class Repository<T>
{
    private readonly List<T> _items = new();

    /// <summary>Кількість елементів у сховищі.</summary>
    public int Count => _items.Count;

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    /// <summary>Видаляє елемент. Повертає false, якщо його не було.</summary>
    public bool Remove(T item) => _items.Remove(item);

    /// <summary>Повертає копію списку, щоб зовнішній код не змінив сховище напряму.</summary>
    public List<T> GetAll() => new(_items);
}
