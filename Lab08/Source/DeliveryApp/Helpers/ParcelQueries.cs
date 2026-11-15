using DeliveryApp.Models;

namespace DeliveryApp.Helpers;

/// <summary>
/// LINQ-запити до відправлень як методи розширення.
/// Усі вони відкладені: нічого не виконується, доки результат не переберуть.
/// </summary>
public static class ParcelQueries
{
    public static IEnumerable<Parcel> WithStatus(this IEnumerable<Parcel> parcels, ParcelStatus status)
        => parcels.Where(p => p.Status == status);

    public static IEnumerable<Parcel> InCity(this IEnumerable<Parcel> parcels, string city)
        => parcels.Where(p => string.Equals(p.City, city, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<Parcel> WeighingAtLeast(this IEnumerable<Parcel> parcels, double weight)
        => parcels.Where(p => p.Weight >= weight);

    /// <summary>Проєкція: номер, місто та вартість доставки.</summary>
    public static IEnumerable<ParcelSummary> ToSummaries(this IEnumerable<Parcel> parcels)
        => parcels.Select(p => new ParcelSummary(p.Number, p.City, p.CalculateDeliveryCost()));
}
