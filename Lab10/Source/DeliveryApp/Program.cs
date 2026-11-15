using System.Globalization;
using System.Text;
using DeliveryApp.Models;
using DeliveryApp.Services;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

DeliveryCenter center = new();
DeliveryService service = new(center);

// --- 1. Реєстрація: коректні, граничні та помилкові дані ---
Console.WriteLine("Реєстрація, коректні й граничні значення:");
Show(service.RegisterStandard("UA-1001", 2.5, "Іван Петренко", "Київ", "вул. Хрещатик, 1"));
Show(service.RegisterStandard("UA-1002", 30.0, "Олена Коваль", "Львів"));   // верхня межа
Show(service.RegisterStandard("UA-1003", 0.01, "Тарас Бойко", "Одеса"));    // нижня межа
Console.WriteLine($"  Відправлень у центрі: {center.Count}");

Console.WriteLine("\nРеєстрація, помилкове введення:");
Show(service.RegisterStandard("UA-1004", 30.01, "Софія Мороз", "Харків"));
Show(service.RegisterStandard("UA-1004", 0, "Софія Мороз", "Харків"));
Show(service.RegisterStandard("UA-1004", -3, "Софія Мороз", "Харків"));
Show(service.RegisterStandard("UA-1004", double.NaN, "Софія Мороз", "Харків"));
Show(service.RegisterStandard("", 5, "Софія Мороз", "Харків"));
Show(service.RegisterStandard("UA-1004", 5, "", "Харків"));
Show(service.RegisterStandard("UA-1004", 5, "Софія Мороз", "Харків", "   "));
Show(service.RegisterStandard("UA-1001", 5, "Дублікат Тестовий", "Харків"));
Console.WriteLine($"  Відправлень у центрі після відмов: {center.Count} (без змін)");

// --- 2. Оновлення доставки ---
Parcel p1001 = center.FindByNumber("UA-1001")!;
Parcel p1002 = center.FindByNumber("UA-1002")!;

int events = 0;
bool? processingDuringEvent = null;
p1001.StatusChanged += (_, _) =>
{
    events++;
    processingDuringEvent = service.IsProcessing;
};

Console.WriteLine("\nОновлення доставки UA-1001, коректний сценарій:");
Show(service.UpdateStage("UA-1001", ParcelStatus.Accepted));
Show(service.UpdateStage("UA-1001", ParcelStatus.InTransit));
Show(service.UpdateStage("UA-1001", ParcelStatus.Delivered));

DateTime? deliveredAt = p1001.DeliveredAt;
Console.WriteLine($"  Подій: {events}, дата вручення встановлена: {deliveredAt is not null}");
Console.WriteLine($"  IsProcessing під час події: {processingDuringEvent}, після операції: {service.IsProcessing}");

Console.WriteLine("\nОновлення доставки, відмови:");
Show(service.UpdateStage("UA-1002", ParcelStatus.Delivered));      // пропуск етапів
Show(service.UpdateStage("UA-1001", ParcelStatus.Accepted));       // повторна видача
Show(service.UpdateStage("UA-1001", ParcelStatus.Delivered));      // повторне вручення
Show(service.UpdateStage("UA-1002", (ParcelStatus)99));            // неіснуючий етап
Show(service.UpdateStage("UA-9999", ParcelStatus.Accepted));       // посилки немає

Console.WriteLine("\nУзгодженість стану після відмов:");
Console.WriteLine($"  UA-1001: {Parcel.Describe(p1001.Status)}, подій усього {events}, дата вручення не змінилась: {p1001.DeliveredAt == deliveredAt}");
Console.WriteLine($"  UA-1002: {Parcel.Describe(p1002.Status)} (не змінилась)");

// --- 3. Власний виняток та неочікувана помилка ---
Console.WriteLine("\nВласний виняток:");
try
{
    p1001.ChangeStatus(ParcelStatus.InTransit);
}
catch (ParcelAlreadyDeliveredException ex)
{
    Console.WriteLine($"  Посилка {ex.ParcelNumber}, спроба: «{Parcel.Describe(ex.AttemptedStatus)}». {ex.Message}");
}

Console.WriteLine("\nНеочікувана помилка (порушення контракту виклику):");
try
{
    service.UpdateStage(null!, ParcelStatus.Accepted);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"  Сервіс її не приховав: параметр «{ex.ParamName}», IsProcessing = {service.IsProcessing}");
}

static void Show(OperationResult<Parcel> result)
    => Console.WriteLine($"  [успіх: {result.IsSuccess}] {result.Message}");
