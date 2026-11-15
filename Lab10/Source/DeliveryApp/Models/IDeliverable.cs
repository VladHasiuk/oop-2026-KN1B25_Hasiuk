namespace DeliveryApp.Models;

/// <summary>
/// Контракт для будь-якого об'єкта, вартість доставки якого можна розрахувати.
/// Дозволяє працювати з посилками та іншими "доставлюваними" сутностями
/// через спільний тип, незалежно від конкретної реалізації.
/// </summary>
public interface IDeliverable
{
    /// <summary>Номер (трек-код) відправлення.</summary>
    string Number { get; }

    /// <summary>Розраховує вартість доставки, грн.</summary>
    decimal CalculateDeliveryCost();

    /// <summary>Формує текстовий опис відправлення.</summary>
    string GetInfo();
}
