namespace DeliveryApp.Models;

/// <summary>
/// Дані події зміни етапу доставки.
/// </summary>
public sealed class ParcelStatusChangedEventArgs : EventArgs
{
    public ParcelStatusChangedEventArgs(
        string parcelNumber, ParcelStatus oldStatus, ParcelStatus newStatus, DateTime changedAt)
    {
        ParcelNumber = parcelNumber;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = changedAt;
    }

    public string ParcelNumber { get; }
    public ParcelStatus OldStatus { get; }
    public ParcelStatus NewStatus { get; }
    public DateTime ChangedAt { get; }
}
