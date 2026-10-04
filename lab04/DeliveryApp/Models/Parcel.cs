namespace DeliveryApp.Models;



// Реалізується інтерфейс <see cref="IDeliverable"/>, щоб будь-яка посилка
//могла оброблятися через спільний контракт без прив'язки до конкретного типу.

public abstract class Parcel : IDeliverable
{
    
    public const double MaxWeight = 30.0;

    // Базова вартість оформлення відправлення 
    protected const decimal BaseFee = 30m;

    //Базовий тариф за 1 кг ваги
    protected const decimal RatePerKg = 20m;

    private string _number = string.Empty;
    private double _weight;
    private string _recipient = string.Empty;

    protected Parcel(string number, double weight, string recipient)
    {
        Number = number;
        Weight = weight;
        Recipient = recipient;
        Status = ParcelStatus.Created;
    }

  
    public abstract string TypeName { get; }


    public string Number
    {
        get => _number;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Номер посилки не може бути порожнім.", nameof(Number));
            }

            _number = value.Trim();
        }
    }


    public double Weight
    {
        get => _weight;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Weight), value, "Вага посилки повинна бути більшою за нуль.");
            }

            if (value > MaxWeight)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Weight), value, $"Вага посилки не може перевищувати {MaxWeight} кг.");
            }

            _weight = value;
        }
    }

    public string Recipient
    {
        get => _recipient;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Ім'я отримувача не може бути порожнім.", nameof(Recipient));
            }

            if (value.Trim().Length < 2)
            {
                throw new ArgumentException("Ім'я отримувача занадто коротке.", nameof(Recipient));
            }

            _recipient = value.Trim();
        }
    }

    public ParcelStatus Status { get; private set; }

    
    // Розраховує вартість доставки. Базова реалізація: оформлення + тариф за кг
   
    public virtual decimal CalculateDeliveryCost() => BaseFee + (decimal)Weight * RatePerKg;

    
    // Формує текстовий опис посилки
   
    public virtual string GetInfo()
        => $"[{TypeName}] № {Number}, вага {Weight} кг, отримувач: {Recipient}, стан: {Describe(Status)}";

   
    // Змінює стан: Створена > Прийнята > В дорозі > Доставлена
  
    public void ChangeStatus(ParcelStatus newStatus)
    {
        if (!CanChangeTo(newStatus))
        {
            throw new InvalidOperationException(
                $"Неможливо змінити стан посилки з «{Describe(Status)}» на «{Describe(newStatus)}».");
        }

        Status = newStatus;
    }

    // Перевіряє чи дозволений перехід у вказаний стан
    public bool CanChangeTo(ParcelStatus newStatus) => Status switch
    {
        ParcelStatus.Created => newStatus == ParcelStatus.Accepted,
        ParcelStatus.Accepted => newStatus == ParcelStatus.InTransit,
        ParcelStatus.InTransit => newStatus == ParcelStatus.Delivered,
        _ => false
    };

    public static string Describe(ParcelStatus status) => status switch
    {
        ParcelStatus.Created => "Створена",
        ParcelStatus.Accepted => "Прийнята",
        ParcelStatus.InTransit => "В дорозі",
        ParcelStatus.Delivered => "Доставлена",
        _ => "Невідомий стан"
    };

    public override string ToString() => GetInfo();
}
