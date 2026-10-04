namespace DeliveryApp.Models;

/// <summary>
/// Кур'єр служби доставки з інкапсульованим станом.
/// </summary>
public class Courier
{
    private string _name = string.Empty;
    private string _phone = string.Empty;

    public Courier(string name, string phone)
    {
        Name = name;
        Phone = phone;
    }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Ім'я кур'єра не може бути порожнім.", nameof(Name));
            }

            _name = value.Trim();
        }
    }

   
    public string Phone
    {
        get => _phone;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Телефон кур'єра не може бути порожнім.", nameof(Phone));
            }

            int digits = value.Count(char.IsDigit);
            if (digits < 10)
            {
                throw new ArgumentException("Некоректний номер телефону кур'єра.", nameof(Phone));
            }

            _phone = value.Trim();
        }
    }

    //Кількість посилок, доставлених кур'єром. Змінюється лише класом
    public int DeliveredCount { get; private set; }

    
    //Кур'єр приймає посилку до доставлення (стан «Прийнята»)
    
    public void AcceptParcel(Parcel parcel)
    {
        ArgumentNullException.ThrowIfNull(parcel);
        parcel.ChangeStatus(ParcelStatus.Accepted);
    }

  
    // Кур'єр вирушає з посилкою (стан «В дорозі»)
    
    public void StartDelivery(Parcel parcel)
    {
        ArgumentNullException.ThrowIfNull(parcel);
        parcel.ChangeStatus(ParcelStatus.InTransit);
    }

   
    // Кур'єр вручає посилку отримувачу (стан «Доставлена»)
    
    public void CompleteDelivery(Parcel parcel)
    {
        ArgumentNullException.ThrowIfNull(parcel);
        parcel.ChangeStatus(ParcelStatus.Delivered);
        DeliveredCount++;
    }

    public override string ToString()
        => $"{Name}, тел. {Phone}, доставлено посилок: {DeliveredCount}";
}
