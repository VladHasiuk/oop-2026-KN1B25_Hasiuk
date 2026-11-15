namespace DeliveryApp.Models;

/// <summary>
/// Спроба повторно видати (змінити етап) посилку, яку вже доставлено.
/// Успадковує InvalidOperationException: стан об'єкта не дозволяє операцію,
/// але власний тип дає змогу обробити цю ситуацію окремо й знати номер посилки.
/// </summary>
public sealed class ParcelAlreadyDeliveredException : InvalidOperationException
{
    public ParcelAlreadyDeliveredException(string parcelNumber, ParcelStatus attemptedStatus)
        : base($"Посилку {parcelNumber} уже доставлено, повторна видача неможлива.")
    {
        ParcelNumber = parcelNumber;
        AttemptedStatus = attemptedStatus;
    }

    public string ParcelNumber { get; }
    public ParcelStatus AttemptedStatus { get; }
}
