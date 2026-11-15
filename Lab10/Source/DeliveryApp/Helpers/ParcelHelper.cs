using DeliveryApp.Models;

namespace DeliveryApp.Helpers;

/// <summary>
/// Узагальнені методи для роботи з посилками.
/// </summary>
public static class ParcelHelper
{
    /// <summary>
    /// Повертає той із двох об'єктів, у якого більше значення selector.
    /// При рівності повертає перший. TKey має вміти порівнювати себе.
    /// </summary>
    public static T ChooseBy<T, TKey>(T first, T second, Func<T, TKey> selector)
        where TKey : IComparable<TKey>
    {
        return selector(first).CompareTo(selector(second)) >= 0 ? first : second;
    }

    /// <summary>
    /// Ліниво повертає елементи, що відповідають умові (ітератор з yield return).
    /// </summary>
    public static IEnumerable<T> Find<T>(IEnumerable<T> items, Func<T, bool> predicate)
    {
        foreach (T item in items)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Описує посилку будь-якого похідного типу. where T : Parcel
    /// дає доступ до спільних властивостей базового класу.
    /// </summary>
    public static string DescribeParcel<T>(T parcel) where T : Parcel
    {
        return $"{parcel.TypeName} № {parcel.Number}, вага {parcel.Weight} кг, " +
               $"отримувач: {parcel.Recipient}, місто: {parcel.City}, вартість: {parcel.CalculateDeliveryCost():F2} грн";
    }

    /// <summary>
    /// Реєструє стандартну посилку. Замість винятку повертає OperationResult.
    /// </summary>
    public static OperationResult<StandardParcel> TryRegisterStandard(
        string number, double weight, string recipient, string city)
    {
        try
        {
            StandardParcel parcel = new(number, weight, recipient, city);
            return OperationResult<StandardParcel>.Success(parcel, $"Посилку {parcel.Number} зареєстровано");
        }
        catch (ArgumentException ex)
        {
            return OperationResult<StandardParcel>.Failure($"Реєстрацію скасовано: {FirstLine(ex.Message)}");
        }
    }

    /// <summary>
    /// Доставляє посилку кур'єром (прийняття → в дорозі → вручення).
    /// Тип результату збігається з типом переданої посилки.
    /// </summary>
    public static OperationResult<T> TryDeliver<T>(Courier courier, T parcel) where T : Parcel
    {
        try
        {
            courier.AcceptParcel(parcel);
            courier.StartDelivery(parcel);
            courier.CompleteDelivery(parcel);
            return OperationResult<T>.Success(parcel, $"Посилку {parcel.Number} доставлено");
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<T>.Failure($"Доставку не виконано: {ex.Message}");
        }
    }

    private static string FirstLine(string text) => text.Split('\n')[0].TrimEnd('\r');
}
