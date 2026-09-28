namespace DeliveryApp.Models;

/// <summary>
/// Крихка посилка: базовий тариф + спеціальне пакування + страхування (2% від оголошеної цінності).
/// </summary>
public class FragileParcel : Parcel
{
    private const decimal PackagingFee = 40m;
    private const decimal InsuranceRate = 0.02m;

    private decimal _insuredValue;

    public FragileParcel(string number, double weight, string recipient, decimal insuredValue)
        : base(number, weight, recipient)
    {
        InsuredValue = insuredValue;
    }

    public override string TypeName => "Крихка";

    /// <summary>Оголошена цінність вмісту, грн. Повинна бути більшою за нуль.</summary>
    public decimal InsuredValue
    {
        get => _insuredValue;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(InsuredValue), value, "Оголошена цінність повинна бути більшою за нуль.");
            }

            _insuredValue = value;
        }
    }

    public override decimal CalculateDeliveryCost()
        => base.CalculateDeliveryCost() + PackagingFee + InsuredValue * InsuranceRate;

    public override string GetInfo()
        => base.GetInfo() + $", оголошена цінність: {InsuredValue:F2} грн, спеціальне пакування";
}
