using System.Globalization;
using System.Text;
using DeliveryApp.Helpers;
using DeliveryApp.Models;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

// --- Підготовка даних ---
DeliveryCenter center = new();
Courier courier = new("Андрій Мельник", "+380671112233");

StandardParcel p1001 = new("UA-1001", 2.5, "Іван Петренко", "Київ")
{
    Address = "вул. Хрещатик, 1",
    RecipientPhone = "+380671110001",
    Size = new Dimensions(30, 20, 10)
};
ExpressParcel p2001 = new("UA-2001", 4.0, "Олена Коваль", "Львів")
{
    RecipientPhone = "+380509998877"
};
FragileParcel p3001 = new("UA-3001", 1.5, "Тарас Бойко", "Київ", 5000m);
StandardParcel p1002 = new("UA-1002", 12.0, "Софія Мороз", "Одеса")
{
    Address = "вул. Дерибасівська, 5",
    Size = new Dimensions(60, 40, 40)
};
ExpressParcel p2002 = new("UA-2002", 8.0, "Максим Литвин", "Львів")
{
    RecipientPhone = "+380931234567"
};

foreach (Parcel parcel in new Parcel[] { p1001, p2001, p3001, p1002, p2002 })
{
    center.Register(parcel);
}

ParcelHelper.TryDeliver(courier, p1001);
courier.AcceptParcel(p2001);
courier.StartDelivery(p2001);
courier.AcceptParcel(p2002);

// --- 1. Знімки: record та record struct ---
ParcelSnapshot[] snapshots = center.GetAll().Select(ParcelSnapshot.FromParcel).ToArray();

Console.WriteLine("Знімки відправлень:");
foreach (ParcelSnapshot s in snapshots)
{
    Console.WriteLine($"  {s.Number} | {s.City} | {s.Weight} кг | {Parcel.Describe(s.Status)} | {s.DeliveryCost:F2} грн");
}

Console.WriteLine("\nСемантика значень:");
ParcelSnapshot a = ParcelSnapshot.FromParcel(p2001);
ParcelSnapshot b = ParcelSnapshot.FromParcel(p2001);
Console.WriteLine($"  Два знімки одного відправлення: a == b → {a == b}, те саме посилання → {ReferenceEquals(a, b)}");

ParcelSnapshot delivered = a with { Status = ParcelStatus.Delivered };
Console.WriteLine($"  Копія через with: у копії {Parcel.Describe(delivered.Status)}, в оригіналі {Parcel.Describe(a.Status)}, a == копія → {a == delivered}");

StandardParcel twinA = new("UA-0001", 1.0, "Тест Тестов", "Київ");
StandardParcel twinB = new("UA-0001", 1.0, "Тест Тестов", "Київ");
Console.WriteLine($"  Два об'єкти класу з однаковими даними: Equals → {twinA.Equals(twinB)}");

p2001.ChangeStatus(ParcelStatus.Delivered);
Console.WriteLine($"  Після доставки посилки знімок лишився «{Parcel.Describe(a.Status)}», сама посилка «{Parcel.Describe(p2001.Status)}»");

Dimensions box = new(30, 20, 10);
Dimensions taller = box with { HeightCm = 15 };
Console.WriteLine($"  record struct: об'єм {box.VolumeCm3} см³, після with {taller.VolumeCm3} см³, оригінал {box.VolumeCm3} см³");
Console.WriteLine($"  Порівняння за значенням: {box == new Dimensions(30, 20, 10)}");

// --- 2. Необов'язкові дані (nullable reference types) ---
Console.WriteLine("\nНеобов'язкові дані:");
foreach (ParcelSnapshot s in snapshots)
{
    Console.WriteLine($"  {s.Number}: адреса — {ParcelSnapshotInfo.GetAddressText(s)}; " +
                      $"контакт — {ParcelSnapshotInfo.GetContactText(s)}; {ParcelSnapshotInfo.GetDeliveryText(s)}; " +
                      $"габарити — {ParcelSnapshotInfo.GetSizeText(s)}");
}

Console.WriteLine("\nПеревірка необов'язкових полів:");
TryRun("Скинути адресу в null", () => p1001.Address = null);
TryRun("Порожня адреса з пробілів", () => p1001.Address = "   ");
TryRun("Короткий телефон", () => p1001.RecipientPhone = "12345");
Console.WriteLine($"  Адреса UA-1001 після цього: {p1001.Address ?? "null"}");

// --- 3. Pattern matching: клас за вагою та статусом ---
Console.WriteLine("\nКласифікація:");
foreach (ParcelSnapshot s in snapshots)
{
    Console.WriteLine($"  {s.Number} ({s.Weight} кг, {Parcel.Describe(s.Status)}) → {ParcelSnapshotInfo.Classify(s)}");
}

// --- 4. Індекси та діапазони ---
Console.WriteLine("\nНомер відправлення (індекси й діапазони):");
foreach (string number in new[] { "UA-1001", "UA-", "UA", "U", "" })
{
    Console.WriteLine($"  \"{number}\" → префікс \"{ParcelSnapshotInfo.GetPrefix(number)}\", " +
                      $"серія \"{ParcelSnapshotInfo.GetSerial(number)}\", " +
                      $"останній символ: {ParcelSnapshotInfo.GetLastChar(number)}");
}

Console.WriteLine("\nДіапазони для масиву знімків:");
Console.WriteLine($"  Перші 3: {Numbers(ParcelSnapshotInfo.FirstN(snapshots, 3))}");
Console.WriteLine($"  Останні 2: {Numbers(ParcelSnapshotInfo.LastN(snapshots, 2))}");
Console.WriteLine($"  Перші 99 (більше, ніж є): {ParcelSnapshotInfo.FirstN(snapshots, 99).Length} шт.");
Console.WriteLine($"  Порожній масив, перші 3: {ParcelSnapshotInfo.FirstN(Array.Empty<ParcelSnapshot>(), 3).Length} шт.");

static string Numbers(ParcelSnapshot[] items) => string.Join(", ", items.Select(s => s.Number));

static void TryRun(string description, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [OK]      {description}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [ПОМИЛКА] {description}: {ex.Message.Split('\n')[0].TrimEnd('\r')}");
    }
}
