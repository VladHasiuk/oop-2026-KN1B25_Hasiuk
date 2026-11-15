namespace DeliveryApp.Models;

/// <summary>
/// Замовлення клієнта на доставку. Підписується на подію посилки
/// і сам оновлює свій статус, коли змінюється етап доставки.
/// </summary>
public class ClientOrder
{
    private readonly List<string> _log = new();

    public ClientOrder(string clientName, Parcel parcel)
    {
        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new ArgumentException("Ім'я клієнта не може бути порожнім.", nameof(clientName));
        }

        ClientName = clientName.Trim();
        Parcel = parcel ?? throw new ArgumentNullException(nameof(parcel));
        StatusText = "Замовлення оформлено";

        // Підписка: тепер OnParcelStatusChanged викликається при кожній зміні стану посилки
        Parcel.StatusChanged += OnParcelStatusChanged;
    }

    public string ClientName { get; }
    public Parcel Parcel { get; }

    /// <summary>Статус замовлення для клієнта. Змінюється лише обробником події.</summary>
    public string StatusText { get; private set; }

    /// <summary>Журнал змін стану, лише для читання.</summary>
    public IReadOnlyList<string> Log => _log;

    /// <summary>Відписка: після неї замовлення більше не реагує на зміни посилки.</summary>
    public void Unsubscribe() => Parcel.StatusChanged -= OnParcelStatusChanged;

    private void OnParcelStatusChanged(object? sender, ParcelStatusChangedEventArgs e)
    {
        StatusText = e.NewStatus switch
        {
            ParcelStatus.Accepted => "Посилку прийнято до відправлення",
            ParcelStatus.InTransit => "Посилка в дорозі",
            ParcelStatus.Delivered => "Посилку доставлено",
            _ => "Замовлення оформлено"
        };

        _log.Add($"{e.ChangedAt:HH:mm:ss} {Parcel.Describe(e.OldStatus)} → {Parcel.Describe(e.NewStatus)}");
    }
}
