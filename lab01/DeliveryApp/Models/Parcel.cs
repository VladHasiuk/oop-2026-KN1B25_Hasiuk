namespace DeliveryApp.Models;

/// <summary>
/// Посилка служби доставки.
/// </summary>
public class Parcel
{
    /// <summary>Номер (трек-код) посилки.</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Вага посилки в кілограмах.</summary>
    public double Weight { get; set; }

    /// <summary>Ім'я отримувача.</summary>
    public string Recipient { get; set; } = string.Empty;
}
