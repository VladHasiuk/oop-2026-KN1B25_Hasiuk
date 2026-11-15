using System.Globalization;
using System.Text;
using DeliveryApp.Helpers;
using DeliveryApp.Models;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

DeliveryCenter center = new();
Courier courier = new("Андрій Мельник", "+380671112233");

Parcel[] data =
{
    new StandardParcel("UA-1001", 2.5, "Іван Петренко", "Київ"),
    new ExpressParcel("UA-2001", 4.0, "Олена Коваль", "Львів"),
    new FragileParcel("UA-3001", 1.5, "Тарас Бойко", "Київ", 5000m),
    new StandardParcel("UA-1002", 12.0, "Софія Мороз", "Одеса"),
    new ExpressParcel("UA-2002", 8.0, "Максим Литвин", "Львів"),
    new StandardParcel("UA-1003", 5.0, "Петро Гаврилюк", "Харків"),
    new FragileParcel("UA-3002", 3.0, "Наталія Гуменюк", "Одеса", 1200m),
    new ExpressParcel("UA-2003", 1.0, "Ірина Кравець", "Київ")
};

foreach (Parcel parcel in data)
{
    center.Register(parcel);
}

IReadOnlyList<Parcel> parcels = center.GetAll();

// --- 1. Делегат ParcelFilter: добір за містом, вагою, статусом ---
Console.WriteLine("Добір за містом (Київ):");
Print(ParcelSelection.FindAll(parcels, ParcelSelection.ByCity("Київ")));

Console.WriteLine("\nДобір за вагою (від 5 кг):");
Print(ParcelSelection.FindAll(parcels, ParcelSelection.ByMinWeight(5)));

Console.WriteLine("\nДве умови разом (Львів і від 5 кг):");
Print(ParcelSelection.FindAll(parcels,
    ParcelSelection.And(ParcelSelection.ByCity("Львів"), ParcelSelection.ByMinWeight(5))));

Console.WriteLine("\nУмова-лямбда безпосередньо (вага до 2 кг):");
Print(ParcelSelection.FindAll(parcels, p => p.Weight < 2));

// Замикання: лямбда захоплює змінну city, а не її значення на момент створення
string city = "Київ";
ParcelFilter byCapturedCity = p => p.City == city;
int first = ParcelSelection.FindAll(parcels, byCapturedCity).Count;
city = "Львів";
int second = ParcelSelection.FindAll(parcels, byCapturedCity).Count;
Console.WriteLine($"\nЗамикання: для Київ {first} шт., після зміни змінної на Львів — {second} шт.");

// --- 2. Подія StatusChanged і підписники ---
Parcel p2001 = center.FindByNumber("UA-2001")!;
Parcel p3001 = center.FindByNumber("UA-3001")!;

ClientOrder order1 = new("Олена Коваль", p2001);
ClientOrder order2 = new("Тарас Бойко", p3001);

// Другий підписник: обробник-лямбда, збережений у змінну, щоб його можна було відписати
EventHandler<ParcelStatusChangedEventArgs> logger = (sender, e) =>
    Console.WriteLine($"    [подія] {e.ParcelNumber}: {Parcel.Describe(e.OldStatus)} → " +
                      $"{Parcel.Describe(e.NewStatus)} о {e.ChangedAt:HH:mm:ss}");
p2001.StatusChanged += logger;
p3001.StatusChanged += logger;

Console.WriteLine($"\nСтатус замовлення {order1.ClientName} до доставки: {order1.StatusText}");

Console.WriteLine("\nДоставка UA-2001:");
ParcelHelper.TryDeliver(courier, p2001);
Console.WriteLine($"  Статус замовлення {order1.ClientName}: {order1.StatusText}");
foreach (string entry in order1.Log)
{
    Console.WriteLine($"    журнал: {entry}");
}

Console.WriteLine("\nUA-3001: прийняття, відписка замовлення, відправлення:");
courier.AcceptParcel(p3001);
Console.WriteLine($"  Статус замовлення {order2.ClientName}: {order2.StatusText}");

order2.Unsubscribe();
courier.StartDelivery(p3001);
Console.WriteLine($"  Після відписки: {order2.StatusText}");
Console.WriteLine($"  Фактичний стан посилки: {Parcel.Describe(p3001.Status)}");

p3001.StatusChanged -= logger;

// Багатоадресний делегат: усі додані методи викликаються по черзі
Action<Parcel> notify = p => Console.WriteLine($"  SMS отримувачу {p.Recipient}: посилка {p.Number}");
notify += p => Console.WriteLine($"  Лист відділенню в м. {p.City}: посилка {p.Number}");
Console.WriteLine("\nБагатоадресний делегат Action<Parcel>:");
notify(p2001);

// --- 3. Func: розрахунок вартості вибраних доставок ---
Console.WriteLine("\nВартість вибраних доставок:");
List<Parcel> kyiv = ParcelSelection.FindAll(parcels, ParcelSelection.ByCity("Київ"));
decimal regular = ParcelSelection.TotalCost(kyiv, p => p.CalculateDeliveryCost());
decimal discounted = ParcelSelection.TotalCost(kyiv, p => p.CalculateDeliveryCost() * 0.9m);
Console.WriteLine($"  Київ ({kyiv.Count} шт.): {regular:F2} грн, зі знижкою 10%: {discounted:F2} грн");

List<Parcel> heavy = ParcelSelection.FindAll(parcels, ParcelSelection.ByMinWeight(5));
Console.WriteLine($"  Від 5 кг ({heavy.Count} шт.): {ParcelSelection.TotalCost(heavy, p => p.CalculateDeliveryCost()):F2} грн");

List<Parcel> inTransit = ParcelSelection.FindAll(parcels, ParcelSelection.ByStatus(ParcelStatus.InTransit));
Console.WriteLine($"  В дорозі ({inTransit.Count} шт.): {ParcelSelection.TotalCost(inTransit, p => p.CalculateDeliveryCost()):F2} грн");

static void Print(IEnumerable<Parcel> items)
{
    foreach (Parcel p in items)
    {
        Console.WriteLine(
            $"  {p.Number} | {p.City} | {Parcel.Describe(p.Status)} | {p.Weight} кг | {p.CalculateDeliveryCost():F2} грн");
    }
}
