namespace DeliveryApp.Models;

/// <summary>
/// Замовлення на доставку. Об'єднує посилку (через інтерфейс <see cref="IDeliverable"/>),
/// кур'єра та дані отримувача, демонструючи композицію: DeliveryOrder не успадковує
/// жоден з цих класів, а лише володіє посиланнями на них і делегує їм роботу.
/// </summary>
public class DeliveryOrder
{
    private readonly List<string> _history = new();

    /// <summary>
    /// Створює замовлення на доставку для конкретної посилки та отримувача.
    /// Кур'єра можна призначити пізніше через <see cref="AssignCourier"/>.
    /// </summary>
    public DeliveryOrder(IDeliverable parcel, string recipientAddress)
    {
        Parcel = parcel ?? throw new ArgumentNullException(nameof(parcel));

        if (string.IsNullOrWhiteSpace(recipientAddress))
        {
            throw new ArgumentException("Адреса отримувача не може бути порожньою.", nameof(recipientAddress));
        }

        RecipientAddress = recipientAddress.Trim();
        CreatedAt = DateTime.Now;
        _history.Add($"{CreatedAt:HH:mm}: замовлення створено для посилки {Parcel.Number}");
    }

    //Посилка, яку потрібно доставити. Зберігається через інтерфейс, а не конкретний тип
    public IDeliverable Parcel { get; }

    //Кур'єр, призначений на це замовлення. Може бути ще не призначений
    public Courier? AssignedCourier { get; private set; }

    //Адреса отримувача для цього конкретного замовлення
    public string RecipientAddress { get; }

    //Час створення замовлення
    public DateTime CreatedAt { get; }

    //Журнал подій по замовленню, лише для читання ззовні
    public IReadOnlyList<string> History => _history;

    //Призначає кур'єра на замовлення
    public void AssignCourier(Courier courier)
    {
        AssignedCourier = courier ?? throw new ArgumentNullException(nameof(courier));
        _history.Add($"{DateTime.Now:HH:mm}: призначено кур'єра {courier.Name}");
    }

  
    public void CompleteDelivery()
    {
        if (AssignedCourier is null)
        {
            throw new InvalidOperationException("Неможливо виконати доставку: кур'єра не призначено.");
        }

        if (Parcel is not Parcel concreteParcel)
        {
            throw new InvalidOperationException("Для керування станом потрібен об'єкт типу Parcel.");
        }

        AssignedCourier.AcceptParcel(concreteParcel);
        _history.Add($"{DateTime.Now:HH:mm}: посилку прийнято кур'єром");

        AssignedCourier.StartDelivery(concreteParcel);
        _history.Add($"{DateTime.Now:HH:mm}: посилка в дорозі");

        AssignedCourier.CompleteDelivery(concreteParcel);
        _history.Add($"{DateTime.Now:HH:mm}: посилку вручено отримувачу");
    }

    public string GetSummary()
    {
        string courierInfo = AssignedCourier is null ? "не призначений" : AssignedCourier.Name;
        return $"Замовлення на {Parcel.Number}: {Parcel.GetInfo()}; " +
               $"адреса: {RecipientAddress}; кур'єр: {courierInfo}; " +
               $"вартість доставки: {Parcel.CalculateDeliveryCost():F2} грн";
    }
}
