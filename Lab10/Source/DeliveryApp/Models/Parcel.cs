namespace DeliveryApp.Models;

/// <summary>
/// Абстрактний базовий клас посилки. Містить спільні властивості, валідацію,
/// керування станом та базовий алгоритм розрахунку вартості доставки.
/// Реалізує інтерфейс <see cref="IDeliverable"/>, тому будь-яка посилка
/// може оброблятися через спільний контракт без прив'язки до конкретного типу.
/// </summary>
public abstract class Parcel : IDeliverable
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
    private string _city = string.Empty;
    private string? _address;
    private string? _recipientPhone;

    protected Parcel(string number, double weight, string recipient, string city)
    {
        Number = number;
        Weight = weight;
        Recipient = recipient;
        City = city;
        Status = ParcelStatus.Created;
    }

    /// <summary>
    /// Подія: змінився етап доставки. Передає номер посилки, старий і новий стан та час зміни.
    /// </summary>
    public event EventHandler<ParcelStatusChangedEventArgs>? StatusChanged;

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
            // NaN не порівнюється з нулем, тому перевіряється окремо
            if (double.IsNaN(value) || value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Weight), value, "Вага посилки повинна бути числом, більшим за нуль.");
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

    /// <summary>Місто призначення. Не може бути порожнім.</summary>
    public string City
    {
        get => _city;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Місто призначення не може бути порожнім.", nameof(City));
            }

            _city = value.Trim();
        }
    }

    /// <summary>Адреса вручення. Необов'язкова: може бути null.</summary>
    public string? Address
    {
        get => _address;
        set
        {
            if (value is not null && string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Адреса, якщо вона вказана, не може бути порожньою.", nameof(Address));
            }

            _address = value?.Trim();
        }
    }

    /// <summary>Телефон одержувача. Необов'язковий; якщо вказано, мінімум 10 цифр.</summary>
    public string? RecipientPhone
    {
        get => _recipientPhone;
        set
        {
            if (value is not null && value.Count(char.IsDigit) < 10)
            {
                throw new ArgumentException("Телефон одержувача повинен містити щонайменше 10 цифр.", nameof(RecipientPhone));
            }

            _recipientPhone = value?.Trim();
        }
    }

    /// <summary>Габарити відправлення. Необов'язкові: якщо не вказані, значення null.</summary>
    public Dimensions? Size { get; set; }

    /// <summary>Дата вручення. Null, доки посилку не доставлено.</summary>
    public DateTime? DeliveredAt { get; private set; }

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
        => $"[{TypeName}] № {Number}, вага {Weight} кг, отримувач: {Recipient}, місто: {City}, стан: {Describe(Status)}";

    /// <summary>
    /// Змінює стан: Створена → Прийнята → В дорозі → Доставлена.
    /// </summary>
    public void ChangeStatus(ParcelStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(newStatus), newStatus, "Невідомий етап доставки.");
        }

        // Усі перевірки виконуються до зміни стану, тому при відмові посилка не змінюється
        if (Status == ParcelStatus.Delivered)
        {
            throw new ParcelAlreadyDeliveredException(Number, newStatus);
        }

        if (!CanChangeTo(newStatus))
        {
            throw new InvalidOperationException(
                $"Неможливо змінити стан посилки з «{Describe(Status)}» на «{Describe(newStatus)}».");
        }

        ParcelStatus oldStatus = Status;
        Status = newStatus;

        if (newStatus == ParcelStatus.Delivered)
        {
            DeliveredAt = DateTime.Now;
        }

        OnStatusChanged(new ParcelStatusChangedEventArgs(Number, oldStatus, newStatus, DateTime.Now));
    }

    /// <summary>Викликає подію StatusChanged для всіх підписників, якщо вони є.</summary>
    protected virtual void OnStatusChanged(ParcelStatusChangedEventArgs e)
        => StatusChanged?.Invoke(this, e);

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
