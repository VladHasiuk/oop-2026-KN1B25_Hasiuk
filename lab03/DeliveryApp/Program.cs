using System.Globalization;
using System.Text;
using DeliveryApp.Models;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("uk-UA");


List<Parcel> parcels = new()
{
    new StandardParcel("UA-1001", 2.5, "Іван Петренко"),
    new ExpressParcel("UA-2001", 4.0, "Олена Коваль"),
    new FragileParcel("UA-3001", 1.5, "Тарас Бойко", 5000m),
    new StandardParcel("UA-1002", 12.0, "Софія Мороз")
};

Courier courier = new("Андрій Мельник", "+380671112233");


Console.WriteLine("Посилки (List<Parcel>):\n");
foreach (Parcel parcel in parcels)
{
    Console.WriteLine(parcel.GetInfo());
    Console.WriteLine($"   Вартість доставки: {parcel.CalculateDeliveryCost():F2} грн");
}

decimal total = parcels.Sum(p => p.CalculateDeliveryCost());
Console.WriteLine($"\nЗагальна вартість доставки: {total:F2} грн");

Parcel mostExpensive = parcels.MaxBy(p => p.CalculateDeliveryCost())!;
Console.WriteLine($"Найдорожча посилка: № {mostExpensive.Number} ({mostExpensive.CalculateDeliveryCost():F2} грн)");


Console.WriteLine("\nПідсумок за типами:");
foreach (var group in parcels.GroupBy(p => p.TypeName))
{
    decimal groupTotal = group.Sum(p => p.CalculateDeliveryCost());
    Console.WriteLine($"{group.Key}: {group.Count()} шт., {groupTotal:F2} грн");
}


Console.WriteLine("\n--- Перевірка валідації ---");
TryRun("Крихка посилка з нульовою цінністю",
    () => new FragileParcel("UA-3002", 1.0, "Петро Гаврилюк", 0m));
TryRun("Стандартна посилка вагою 50 кг",
    () => new StandardParcel("UA-1003", 50, "Петро Гаврилюк"));
TryRun("Експрес-посилка з порожнім отримувачем",
    () => new ExpressParcel("UA-2002", 2.0, " "));


Console.WriteLine("\n--- Доставка експрес-посилки ---");
Parcel express = parcels[1];

courier.AcceptParcel(express);
Console.WriteLine($"{courier.Name}: посилка {express.Number} → {Parcel.Describe(express.Status)}");

courier.StartDelivery(express);
Console.WriteLine($"{courier.Name}: посилка {express.Number} → {Parcel.Describe(express.Status)}");

courier.CompleteDelivery(express);
Console.WriteLine($"{courier.Name}: посилка {express.Number} → {Parcel.Describe(express.Status)}");

Console.WriteLine($"\nПідсумок: {courier}");

static void TryRun(string description, Action action)
{
    try
    {
        action();
        Console.WriteLine($"[OK]      {description}");
    }
    catch (Exception ex)
    {
        string firstLine = ex.Message.Split('\n')[0].TrimEnd('\r');
        Console.WriteLine($"[ПОМИЛКА] {description}: {firstLine}");
    }
}
