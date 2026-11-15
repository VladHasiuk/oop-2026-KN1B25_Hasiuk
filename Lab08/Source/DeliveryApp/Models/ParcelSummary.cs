namespace DeliveryApp.Models;

/// <summary>
/// Коротке представлення відправлення (результат проєкції через Select).
/// </summary>
public record ParcelSummary(string Number, string City, decimal Cost);
