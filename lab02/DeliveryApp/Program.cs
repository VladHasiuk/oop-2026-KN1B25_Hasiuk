using System.Text;
using DeliveryApp.Models;

Console.OutputEncoding = Encoding.UTF8;

// 1. Створення коректних об'єктів
Parcel parcel1 = new("UA-1001", 2.5, "Іван Петренко");
Parcel parcel2 = new("UA-1002", 12.0, "Олена Коваль");

Courier courier1 = new("Андрій Мельник", "+380671112233");
Courier courier2 = new("Марія Шевчук", "+380509998877");

Console.WriteLine("Посилки:");
Console.WriteLine(parcel1);
Console.WriteLine(parcel2);

Console.WriteLine("\nКур'єри:");
Console.WriteLine(courier1);
Console.WriteLine(courier2);

// 2. Валідація даних: спроби записати некоректні значення
Console.WriteLine("\n--- Перевірка валідації ---");

TryChange("Порожній номер посилки", () => parcel1.Number = "   ");
TryChange("Від'ємна вага", () => parcel1.Weight = -5);
TryChange("Вага понад ліміт", () => parcel1.Weight = 100);
TryChange("Порожнє ім'я отримувача", () => parcel1.Recipient = "");
TryChange("Некоректний телефон кур'єра", () => courier1.Phone = "123");

Console.WriteLine($"\nСтан об'єкта не зіпсовано: {parcel1}");

// 3. Керування станом посилки
Console.WriteLine("\n--- Життєвий цикл посилки ---");

courier1.AcceptParcel(parcel1);
Console.WriteLine($"Після прийняття: {Parcel.Describe(parcel1.Status)}");

courier1.StartDelivery(parcel1);
Console.WriteLine($"Після відправлення: {Parcel.Describe(parcel1.Status)}");

courier1.CompleteDelivery(parcel1);
Console.WriteLine($"Після вручення: {Parcel.Describe(parcel1.Status)}");

// 4. Заборонені переходи між станами
Console.WriteLine("\n--- Некоректні переходи ---");

TryChange("Доставлену посилку повернути в дорогу",
    () => parcel1.ChangeStatus(ParcelStatus.InTransit));

TryChange("Щойно створену посилку одразу доставити",
    () => parcel2.ChangeStatus(ParcelStatus.Delivered));

// Пряма зміна стану заборонена компілятором:
// parcel1.Status = ParcelStatus.Created; // помилка компіляції (set is private)

Console.WriteLine($"\nПідсумок: {courier1}");
Console.WriteLine($"Посилка 2 залишилась у стані: {Parcel.Describe(parcel2.Status)}");

// Допоміжний метод для демонстрації обробки помилок
static void TryChange(string description, Action action)
{
    try
    {
        action();
        Console.WriteLine($"[OK]    {description}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ПОМИЛКА] {description}: {ex.Message}");
    }
}
