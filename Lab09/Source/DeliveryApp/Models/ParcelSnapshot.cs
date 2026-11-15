namespace DeliveryApp.Models;

/// <summary>
/// Незмінний «знімок» відправлення на момент створення. Це record: порівнюється
/// за значеннями, копіюється через with. Необов'язкові дані позначені знаком ?.
/// </summary>
public sealed record ParcelSnapshot(
    string Number,
    string TypeName,
    string City,
    double Weight,
    ParcelStatus Status,
    decimal DeliveryCost,
    string? Address,
    string? RecipientPhone,
    DateTime? DeliveredAt,
    Dimensions? Size)
{
    /// <summary>Створює знімок із наявного відправлення.</summary>
    public static ParcelSnapshot FromParcel(Parcel parcel)
    {
        ArgumentNullException.ThrowIfNull(parcel);

        return new ParcelSnapshot(
            parcel.Number,
            parcel.TypeName,
            parcel.City,
            parcel.Weight,
            parcel.Status,
            parcel.CalculateDeliveryCost(),
            parcel.Address,
            parcel.RecipientPhone,
            parcel.DeliveredAt,
            parcel.Size);
    }
}
