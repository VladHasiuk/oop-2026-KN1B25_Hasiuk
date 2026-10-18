namespace DeliveryApp.Models;

/// <summary>
/// Можливі стани посилки.
/// </summary>
public enum ParcelStatus
{
    /// <summary>Створена.</summary>
    Created,

    /// <summary>Прийнята до відправлення.</summary>
    Accepted,

    /// <summary>В дорозі.</summary>
    InTransit,

    /// <summary>Доставлена.</summary>
    Delivered
}
