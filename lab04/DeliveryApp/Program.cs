using System.Globalization;
using System.Text;
using DeliveryApp.Models;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

List<Parcel> parcels = new()
{
    new StandardParcel("UA-1001", 2.5, "Іван Петренко"),
    new ExpressParcel("UA-2001", 4.0, "Олена Коваль"),
    new FragileParcel("UA-3001", 1.5, "Тарас Бойко", 5000m)
};

Courier courier1 = new("Андрій Мельник", "+380671112233");
Courier courier2 = new("Марія Шевчук", "+380509998877");

// --- Завдання 3: поліморфна робота через інтерфейс IDeliverable ---
// Parcel реалізує IDeliverable, тому всі три типи посилок можна зберігати
Console.WriteLine("Посилки через List<IDeliverable>:\n");

List<IDeliverable> deliverables = parcels.Cast<IDeliverable>().ToList();

decimal total = 0;
foreach (IDeliverable item in deliverables)
{
    Console.WriteLine(item.GetInfo());
    decimal cost = item.CalculateDeliveryCost();
    Console.WriteLine($"   Вартість доставки: {cost:F2} грн");
    total += cost;
}

Console.WriteLine($"\nЗагальна вартість доставки: {total:F2} грн");

// --- Завдання 2: композиція у класі DeliveryOrder ---
// DeliveryOrder не успадковує Parcel чи Courier, а лише містить посилання

Console.WriteLine("\n--- Замовлення на доставку (композиція) ---\n");

DeliveryOrder order1 = new(deliverables[0], "м. Київ, вул. Хрещатик, 1");
order1.AssignCourier(courier1);
order1.CompleteDelivery();
Console.WriteLine(order1.GetSummary());
Console.WriteLine("Журнал замовлення:");
foreach (string entry in order1.History)
{
    Console.WriteLine($"  - {entry}");
}

DeliveryOrder order2 = new(deliverables[1], "м. Львів, пр. Свободи, 10");
order2.AssignCourier(courier2);
order2.CompleteDelivery();
Console.WriteLine($"\n{order2.GetSummary()}");

DeliveryOrder order3 = new(deliverables[2], "м. Одеса, вул. Дерибасівська, 5");
Console.WriteLine($"\n{order3.GetSummary()}");

TryRun("Доставка без призначеного кур'єра", () => order3.CompleteDelivery());

Console.WriteLine($"\nПідсумок роботи кур'єрів:");
Console.WriteLine(courier1);
Console.WriteLine(courier2);

static void TryRun(string description, Action action)
{
    try
    {
        action();
        Console.WriteLine($"[OK]      {description}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ПОМИЛКА] {description}: {ex.Message}");
    }
}
