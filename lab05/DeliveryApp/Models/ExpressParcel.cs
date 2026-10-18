namespace DeliveryApp.Models;

/// <summary>
/// Експрес-посилка: терміновий тариф (×1,8 до базового) та фіксована надбавка за пріоритет.
/// </summary>
public class ExpressParcel : Parcel
{
    private const decimal ExpressMultiplier = 1.8m;
    private const decimal PriorityFee = 50m;

    public ExpressParcel(string number, double weight, string recipient)
        : base(number, weight, recipient)
    {
    }

    public override string TypeName => "Експрес";

    public override decimal CalculateDeliveryCost()
        => base.CalculateDeliveryCost() * ExpressMultiplier + PriorityFee;

    public override string GetInfo() => base.GetInfo() + ", строк: до 24 годин";
}
