namespace DeliveryApp.Models;


public abstract class Parcel
{
    /// <summary>Максимально допустима вага посилки, кг.</summary>
    public const double MaxWeight = 30.0;

    /// <summary>Базова вартість оформлення відправлення, грн.</summary>
    protected const decimal BaseFee = 30m;

    /// <summary>Базовий тариф за 1 кг ваги, грн.</summary>
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

    /// <summary>Назва типу посилки. Кожен похідний клас задає власну.</summary>
    public abstract string TypeName { get; }

    /// <summary>Номер (трек-код) посилки. Не може бути порожнім.</summary>
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

    /// <summary>Вага посилки в кілограмах. Повинна бути більшою за нуль.</summary>
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

    /// <summary>Ім'я отримувача. Не може бути порожнім, мінімум два символи.</summary>
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

    /// <summary>Поточний стан. Змінюється лише через <see cref="ChangeStatus"/>.</summary>
    public ParcelStatus Status { get; private set; }

    /// <summary>
    /// Розраховує вартість доставки. Базова реалізація: оформлення + тариф за кілограм.
    /// Похідні класи перевизначають алгоритм.
    /// </summary>
    public virtual decimal CalculateDeliveryCost() => BaseFee + (decimal)Weight * RatePerKg;

    /// <summary>
    /// Формує текстовий опис посилки. Похідні класи доповнюють його своїми даними.
    /// </summary>
    public virtual string GetInfo()
        => $"[{TypeName}] № {Number}, вага {Weight} кг, отримувач: {Recipient}, стан: {Describe(Status)}";

    /// <summary>
    /// Змінює стан: Створена → Прийнята → В дорозі → Доставлена.
    /// </summary>
    public void ChangeStatus(ParcelStatus newStatus)
    {
        if (!CanChangeTo(newStatus))
        {
            throw new InvalidOperationException(
                $"Неможливо змінити стан посилки з «{Describe(Status)}» на «{Describe(newStatus)}».");
        }

        Status = newStatus;
    }

    /// <summary>Перевіряє, чи дозволений перехід у вказаний стан.</summary>
    public bool CanChangeTo(ParcelStatus newStatus) => Status switch
    {
        ParcelStatus.Created => newStatus == ParcelStatus.Accepted,
        ParcelStatus.Accepted => newStatus == ParcelStatus.InTransit,
        ParcelStatus.InTransit => newStatus == ParcelStatus.Delivered,
        _ => false
    };

    /// <summary>Повертає назву стану українською.</summary>
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
