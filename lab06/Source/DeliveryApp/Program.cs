using System.Globalization;
using System.Text;
using DeliveryApp.Helpers;
using DeliveryApp.Models;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

DeliveryCenter center = new();
Courier courier = new("Андрій Мельник", "+380671112233");

// --- 1. Реєстрація відправлень (Repository<T> + Dictionary) ---
Console.WriteLine("Реєстрація:");
Parcel[] newParcels =
{
    new StandardParcel("UA-1001", 2.5, "Іван Петренко"),
    new ExpressParcel("UA-2001", 4.0, "Олена Коваль"),
    new FragileParcel("UA-3001", 1.5, "Тарас Бойко", 5000m),
    new StandardParcel("UA-1002", 12.0, "Софія Мороз"),
    new ExpressParcel("UA-2002", 8.0, "Максим Литвин")
};

foreach (Parcel parcel in newParcels)
{
    Show(center.Register(parcel));
}

// Дублікат номера: словник не допускає однакових ключів
Show(center.Register(new StandardParcel("UA-1001", 1.0, "Дублікат Тестовий")));
Console.WriteLine($"Усього відправлень: {center.Count}");

// --- 2. Пошук за номером: Dictionary<string, Parcel> ---
Console.WriteLine("\nПошук за номером:");
Parcel? found = center.FindByNumber("UA-2001");
Console.WriteLine($"  UA-2001: {(found is null ? "не знайдено" : ParcelHelper.DescribeParcel(found))}");
Console.WriteLine($"  XX-0000: {(center.FindByNumber("XX-0000") is null ? "не знайдено" : "знайдено")}");

// --- 3. Черга доставки: Queue<Parcel> (FIFO) ---
Console.WriteLine("\nПостановка в чергу:");
Show(center.EnqueueForDelivery("UA-2001"));
Show(center.EnqueueForDelivery("UA-3001"));
Show(center.EnqueueForDelivery("UA-1001"));
Show(center.EnqueueForDelivery("UA-1001"));
Show(center.EnqueueForDelivery("XX-0000"));

Console.WriteLine($"Довжина черги: {center.QueueLength}");
Console.WriteLine("Порядок доставки:");
PrintList(center.GetQueue());

Console.WriteLine("\nДоставка з черги:");
Show(center.DeliverNext(courier));
Show(center.DeliverNext(courier));
Console.WriteLine($"Залишилось у черзі: {center.QueueLength}");

// --- 4. Ітератор yield return: відправлення за статусом ---
Console.WriteLine("\nДоставлені:");
PrintList(center.GetByStatus(ParcelStatus.Delivered));

Console.WriteLine("\nСтворені (ще не оброблені):");
PrintList(center.GetByStatus(ParcelStatus.Created));

// --- 5. Узагальнений метод Find<T> з yield та умовою ---
Console.WriteLine("\nВідправлення вагою від 5 кг:");
PrintList(ParcelHelper.Find(center.GetAll(), p => p.Weight >= 5));

// --- 6. Видалення ---
Console.WriteLine("\nВидалення:");
Show(center.Remove("UA-1002"));
Show(center.Remove("UA-1001"));
Console.WriteLine($"Залишилось відправлень: {center.Count}");

Console.WriteLine($"\nПідсумок: {courier}");

static void Show(OperationResult<Parcel> result)
    => Console.WriteLine($"  [успіх: {result.IsSuccess}] {result.Message}");

static void PrintList(IEnumerable<Parcel> parcels)
{
    foreach (Parcel p in parcels)
    {
        Console.WriteLine($"  {p.Number} | {p.TypeName} | {Parcel.Describe(p.Status)}");
    }
}
