using DeliveryApp.Models;

namespace DeliveryApp.Helpers;

/// <summary>
/// Класифікація та безпечне опрацювання даних знімка:
/// pattern matching, nullable-значення, індекси й діапазони.
/// </summary>
public static class ParcelSnapshotInfo
{
    // --- Pattern matching: клас за вагою та статусом ---
    public static string Classify(ParcelSnapshot s) => s switch
    {
        { Status: ParcelStatus.Delivered } => "Завершено",
        { Weight: < 2.0, Status: ParcelStatus.Created } => "Легка, очікує приймання",
        { Weight: < 2.0 } => "Легка, у роботі",
        { Weight: < 10.0, Status: ParcelStatus.Accepted or ParcelStatus.InTransit } => "Середня, у русі",
        { Weight: < 10.0 } => "Середня, очікує відправлення",
        { Status: ParcelStatus.Created } => "Важка, потребує планування",
        _ => "Важка, у русі"
    };

    // --- Nullable: кожне необов'язкове поле перевіряється перед використанням ---
    public static string GetAddressText(ParcelSnapshot s)
        => s.Address is { Length: > 0 } address ? address : "не вказано";

    public static string GetContactText(ParcelSnapshot s)
        => s.RecipientPhone is { } phone
            ? $"тел. ***{(phone.Length >= 4 ? phone[^4..] : phone)}"   // остання частина номера через індекс з кінця
            : "відсутній";

    public static string GetDeliveryText(ParcelSnapshot s)
        => s.DeliveredAt is { } at ? $"вручено {at:dd.MM.yyyy}" : "ще не вручено";

    public static string GetSizeText(ParcelSnapshot s)
        => s.Size is { } size
            ? $"{size.LengthCm}×{size.WidthCm}×{size.HeightCm} см ({size.VolumeCm3} см³)"
            : "не вказано";

    // --- Індекси та діапазони для номера відправлення (формат UA-1001) ---
    public static string GetPrefix(string number)
        => number.Length >= 2 ? number[..2] : number;

    public static string GetSerial(string number)
    {
        int dash = number.IndexOf('-');
        return dash < 0 ? number : number[(dash + 1)..];
    }

    public static string GetLastChar(string number)
        => number.Length > 0 ? number[^1].ToString() : "немає";

    // --- Діапазони для масивів ---
    public static T[] FirstN<T>(T[] items, int count)
        => items[..Math.Min(count, items.Length)];

    public static T[] LastN<T>(T[] items, int count)
        => items[^Math.Min(count, items.Length)..];
}
