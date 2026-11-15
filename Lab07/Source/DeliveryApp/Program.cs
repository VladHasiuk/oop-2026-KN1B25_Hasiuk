using System.Globalization;
using System.Text;
using DeliveryApp.Helpers;
using DeliveryApp.Models;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

// --- Підготовка даних: вісім відправлень у різних містах і станах ---
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

// Три доставлено, дві в дорозі, одна прийнята, дві створені
foreach (string number in new[] { "UA-1001", "UA-2001", "UA-3001" })
{
    ParcelHelper.TryDeliver(courier, center.FindByNumber(number)!);
}

foreach (string number in new[] { "UA-1002", "UA-2002" })
{
    courier.AcceptParcel(center.FindByNumber(number)!);
    courier.StartDelivery(center.FindByNumber(number)!);
}

courier.AcceptParcel(center.FindByNumber("UA-1003")!);

IReadOnlyList<Parcel> parcels = center.GetAll();

Console.WriteLine("Усі відправлення:");
Print(parcels);

// --- 1. Фільтрація та сортування (Where, OrderBy, ThenBy) ---
Console.WriteLine("\nДоставлені, за номером:");
Print(parcels.WithStatus(ParcelStatus.Delivered).OrderBy(p => p.Number));

Console.WriteLine("\nУ Київ, від дорожчих до дешевших:");
Print(parcels.InCity("Київ").OrderByDescending(p => p.CalculateDeliveryCost()));

Console.WriteLine("\nВагою від 5 кг, від важчих:");
Print(parcels.WeighingAtLeast(5).OrderByDescending(p => p.Weight).ThenBy(p => p.Number));

Console.WriteLine("\nЛанцюжок умов: у Львів і в дорозі:");
Print(parcels.InCity("Львів").WithStatus(ParcelStatus.InTransit));

// Той самий запит про Київ у синтаксисі запитів (query syntax)
var kyivQuery = from p in parcels
                where p.City == "Київ"
                orderby p.CalculateDeliveryCost() descending
                select new { p.Number, Cost = p.CalculateDeliveryCost() };

Console.WriteLine("\nКиїв (синтаксис запитів):");
foreach (var item in kyivQuery)
{
    Console.WriteLine($"  {item.Number} — {item.Cost:F2} грн");
}

// --- 2. Проєкція (Select) ---
Console.WriteLine("\nПроєкція: номер, місто, вартість:");
foreach (ParcelSummary s in parcels.ToSummaries().OrderBy(s => s.Number))
{
    Console.WriteLine($"  {s.Number} → {s.City}, {s.Cost:F2} грн");
}

// --- 3. Групування та агрегування (GroupBy) ---
Console.WriteLine("\nГрупування за статусом:");
var byStatus = parcels
    .GroupBy(p => p.Status)
    .Select(g => new
    {
        Status = g.Key,
        Count = g.Count(),
        Total = g.Sum(p => p.CalculateDeliveryCost())
    })
    .OrderBy(x => x.Status);

foreach (var g in byStatus)
{
    Console.WriteLine($"  {Parcel.Describe(g.Status)}: {g.Count} шт., разом {g.Total:F2} грн");
}

Console.WriteLine("\nГрупування за містом (склад груп):");
var byCity = parcels.GroupBy(p => p.City).OrderBy(g => g.Key);

foreach (IGrouping<string, Parcel> group in byCity)
{
    string numbers = string.Join(", ", group.Select(p => p.Number));
    Console.WriteLine($"  {group.Key}: {group.Count()} шт., разом {group.Sum(p => p.CalculateDeliveryCost()):F2} грн [{numbers}]");
}

// SelectMany «розгортає» послідовність груп в одну послідовність елементів
Console.WriteLine("\nНайдешевше відправлення в кожне місто (SelectMany):");
Print(byCity.SelectMany(g => g.OrderBy(p => p.CalculateDeliveryCost()).Take(1)));

// --- 4. Агрегування ---
Console.WriteLine("\nАгрегування:");
Console.WriteLine($"  Кількість: {parcels.Count()}");
Console.WriteLine($"  Загальна вартість: {parcels.Sum(p => p.CalculateDeliveryCost()):F2} грн");
Console.WriteLine($"  Середня вартість: {parcels.Average(p => p.CalculateDeliveryCost()):F2} грн");
Console.WriteLine($"  Мінімальна вага: {parcels.Min(p => p.Weight)} кг, максимальна: {parcels.Max(p => p.Weight)} кг");

Parcel mostExpensive = parcels.MaxBy(p => p.CalculateDeliveryCost())!;
Console.WriteLine($"  Найдорожча: {mostExpensive.Number} ({mostExpensive.CalculateDeliveryCost():F2} грн)");
Console.WriteLine($"  Усі номери (Aggregate): {parcels.Select(p => p.Number).Aggregate((acc, n) => acc + ", " + n)}");

// --- 5. Відкладене та негайне виконання ---
Console.WriteLine("\nВідкладене виконання:");
List<Parcel> demo = new()
{
    center.FindByNumber("UA-1001")!,
    center.FindByNumber("UA-1002")!,
    center.FindByNumber("UA-2002")!
};

IEnumerable<Parcel> heavy = demo.Where(p =>
{
    Console.WriteLine($"    перевіряється {p.Number}");
    return p.Weight >= 5;
});

Console.WriteLine("  Запит створено, але ще не виконано");

List<Parcel> snapshot = heavy.ToList();   // тут запит виконується вперше
Console.WriteLine($"  Після ToList(): у знімку {snapshot.Count} шт.");

demo.Add(new StandardParcel("UA-9001", 20.0, "Нова Особа", "Харків"));

int countNow = heavy.Count();             // запит виконується знову, вже з новим елементом
Console.WriteLine($"  Після додавання в джерело: запит дає {countNow} шт., знімок лишився {snapshot.Count} шт.");

// ToDictionary — ще одна негайна операція
Dictionary<string, decimal> costByNumber = parcels.ToDictionary(p => p.Number, p => p.CalculateDeliveryCost());
Console.WriteLine($"  Вартість UA-2002 зі словника: {costByNumber["UA-2002"]:F2} грн");

static void Print(IEnumerable<Parcel> items)
{
    foreach (Parcel p in items)
    {
        Console.WriteLine(
            $"  {p.Number} | {p.City} | {Parcel.Describe(p.Status)} | {p.Weight} кг | {p.CalculateDeliveryCost():F2} грн");
    }
}
