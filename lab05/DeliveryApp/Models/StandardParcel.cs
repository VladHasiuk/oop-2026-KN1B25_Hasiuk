namespace DeliveryApp.Models;

/// <summary>
/// Стандартна посилка: базовий тариф, знижка 10% для важких відправлень (від 10 кг).
/// </summary>
public class StandardParcel : Parcel
{
    /// <summary>Вага, починаючи з якої діє знижка, кг.</summary>
    public const double DiscountWeight = 10.0;

    public StandardParcel(string number, double weight, string recipient)
        : base(number, weight, recipient)
    {
    }

    public override string TypeName => "Стандартна";

    public override decimal CalculateDeliveryCost()
    {
        decimal cost = base.CalculateDeliveryCost();
        return Weight >= DiscountWeight ? cost * 0.9m : cost;
    }

    public override string GetInfo() => base.GetInfo() + ", строк: 3–5 днів";
}
