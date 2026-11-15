namespace DeliveryApp.Models;

/// <summary>
/// Габарити відправлення, см. Невелике незмінне значення без власної ідентичності,
/// тому це record struct: копіюється за значенням, порівнюється за вмістом.
/// </summary>
public readonly record struct Dimensions(double LengthCm, double WidthCm, double HeightCm)
{
    public double VolumeCm3 => LengthCm * WidthCm * HeightCm;
}
