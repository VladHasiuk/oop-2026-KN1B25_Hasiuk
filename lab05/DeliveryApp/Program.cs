using System.Globalization;
using System.Text;
using DeliveryApp.Helpers;
using DeliveryApp.Models;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

// --- 1. Repository<T> для посилок різних типів ---
StandardParcel standard1 = new("UA-1001", 2.5, "Іван Петренко");
ExpressParcel express1 = new("UA-2001", 4.0, "Олена Коваль");
FragileParcel fragile1 = new("UA-3001", 1.5, "Тарас Бойко", 5000m);
StandardParcel standard2 = new("UA-1002", 12.0, "Софія Мороз");
ExpressParcel express2 = new("UA-2002", 8.0, "Максим Литвин");

Repository<Parcel> parcels = new();
parcels.Add(standard1);
parcels.Add(express1);
parcels.Add(fragile1);
parcels.Add(standard2);
parcels.Add(express2);

Console.WriteLine($"Усього посилок: {parcels.Count}");
ParcelHelper.DisplayParcels(parcels.GetAll());

// Окреме сховище лише для крихких посилок (тип перевіряється компілятором)
Repository<FragileParcel> fragileOnly = new();
fragileOnly.Add(fragile1);
fragileOnly.Add(new FragileParcel("UA-3002", 3.0, "Наталія Гуменюк", 1200m));
// fragileOnly.Add(standard1); // помилка компіляції: StandardParcel не є FragileParcel

Console.WriteLine($"\nКрихких посилок: {fragileOnly.Count}");
ParcelHelper.DisplayParcels(fragileOnly.GetAll());

// Той самий клас працює й з іншим типом
Repository<Courier> couriers = new();
Courier courier = new("Андрій Мельник", "+380671112233");
couriers.Add(courier);
couriers.Add(new Courier("Марія Шевчук", "+380509998877"));
Console.WriteLine($"\nКур'єрів: {couriers.Count}");

// --- 2. Узагальнений пошук Find<T> ---
courier.AcceptParcel(standard1);
courier.StartDelivery(standard1);
courier.CompleteDelivery(standard1);

Console.WriteLine("\nПосилки вагою від 5 кг:");
ParcelHelper.DisplayParcels(ParcelHelper.Find(parcels.GetAll(), p => p.Weight >= 5));

Console.WriteLine("\nДоставлені посилки:");
ParcelHelper.DisplayParcels(
    ParcelHelper.Find(parcels.GetAll(), p => p.Status == ParcelStatus.Delivered));

Console.WriteLine("\nПосилки дорожче 200 грн:");
ParcelHelper.DisplayParcels(
    ParcelHelper.Find(parcels.GetAll(), p => p.CalculateDeliveryCost() > 200m));

// Find<T> з іншим типом: пошук кур'єра за іменем
List<Courier> foundCouriers = ParcelHelper.Find(couriers.GetAll(), c => c.Name.StartsWith("Марія"));
Console.WriteLine($"\nЗнайдено кур'єрів: {foundCouriers.Count} ({foundCouriers[0].Name})");

// --- 3. Метод з обмеженням where T : Parcel ---
Console.WriteLine($"\nВартість усіх посилок: {ParcelHelper.GetTotalCost(parcels.GetAll()):F2} грн");
Console.WriteLine($"Вартість крихких посилок: {ParcelHelper.GetTotalCost(fragileOnly.GetAll()):F2} грн");
// ParcelHelper.GetTotalCost(couriers.GetAll()); // помилка компіляції: Courier не є Parcel

// --- Видалення ---
bool removed = parcels.Remove(express2);
Console.WriteLine($"\nВидалено {express2.Number}: {removed}, залишилось: {parcels.Count}");
Console.WriteLine($"Повторне видалення: {parcels.Remove(express2)}");
