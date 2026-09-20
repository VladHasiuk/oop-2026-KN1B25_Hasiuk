using System.Text;
using DeliveryApp.Models;

Console.OutputEncoding = Encoding.UTF8;

Parcel parcel1 = new()
{
    Number = "UA-1001",
    Weight = 2.5,
    Recipient = "Іван Петренко"
};

Parcel parcel2 = new()
{
    Number = "UA-1002",
    Weight = 12.0,
    Recipient = "Олена Коваль"
};

Courier courier1 = new()
{
    Name = "Андрій Мельник"
};

Courier courier2 = new()
{
    Name = "Марія Шевчук"
};

Console.WriteLine("Посилки:");
Console.WriteLine($"№ {parcel1.Number}, вага {parcel1.Weight} кг, отримувач: {parcel1.Recipient}");
Console.WriteLine($"№ {parcel2.Number}, вага {parcel2.Weight} кг, отримувач: {parcel2.Recipient}");

Console.WriteLine("\nКур'єри:");
Console.WriteLine(courier1.Name);
Console.WriteLine(courier2.Name);
