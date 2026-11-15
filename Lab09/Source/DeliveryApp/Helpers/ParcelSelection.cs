using DeliveryApp.Models;

namespace DeliveryApp.Helpers;

/// <summary>
/// Делегат умови добору відправлення. Те саме, що Predicate&lt;Parcel&gt;,
/// але з назвою, зрозумілою в предметній області.
/// </summary>
public delegate bool ParcelFilter(Parcel parcel);

/// <summary>
/// Добір відправлень за умовою, переданою як делегат.
/// </summary>
public static class ParcelSelection
{
    /// <summary>Повертає відправлення, для яких делегат дає true.</summary>
    public static List<Parcel> FindAll(IEnumerable<Parcel> parcels, ParcelFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        List<Parcel> result = new();
        foreach (Parcel parcel in parcels)
        {
            if (filter(parcel))
            {
                result.Add(parcel);
            }
        }

        return result;
    }

    // Лямбди нижче "захоплюють" параметри (city, status, minWeight): замикання

    public static ParcelFilter ByCity(string city)
        => p => string.Equals(p.City, city, StringComparison.OrdinalIgnoreCase);

    public static ParcelFilter ByStatus(ParcelStatus status)
        => p => p.Status == status;

    public static ParcelFilter ByMinWeight(double minWeight)
        => p => p.Weight >= minWeight;

    /// <summary>Об'єднує дві умови: відправлення має відповідати обом.</summary>
    public static ParcelFilter And(ParcelFilter first, ParcelFilter second)
        => p => first(p) && second(p);

    /// <summary>
    /// Сума за правилом, яке передано делегатом Func: вартість можна
    /// рахувати по-різному, не змінюючи сам метод.
    /// </summary>
    public static decimal TotalCost(IEnumerable<Parcel> parcels, Func<Parcel, decimal> price)
        => parcels.Sum(price);
}
