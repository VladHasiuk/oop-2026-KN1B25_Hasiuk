using DeliveryApp.Models;

namespace DeliveryApp.Helpers;

/// <summary>
/// Узагальнені методи для роботи з колекціями.
/// </summary>
public static class ParcelHelper
{
    /// <summary>Повертає елементи, що відповідають умові. Працює з будь-яким типом T.</summary>
    public static List<T> Find<T>(IEnumerable<T> items, Func<T, bool> predicate)
        => items.Where(predicate).ToList();

    /// <summary>Виводить список посилок. where T : Parcel дозволяє користуватись її членами.</summary>
    public static void DisplayParcels<T>(IEnumerable<T> items) where T : Parcel
    {
        foreach (T item in items)
        {
            Console.WriteLine(
                $"  {item.Number} | {item.TypeName} | {item.Weight} кг | " +
                $"{item.CalculateDeliveryCost():F2} грн");
        }
    }

    /// <summary>Сумарна вартість доставки посилок будь-якого похідного типу.</summary>
    public static decimal GetTotalCost<T>(IEnumerable<T> items) where T : Parcel
        => items.Sum(p => p.CalculateDeliveryCost());
}
