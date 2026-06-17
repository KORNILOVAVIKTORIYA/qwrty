namespace DriverMatching.Core.Models;

/// <summary>
/// Водитель с уникальным идентификатором и текущим положением на карте.
/// </summary>
public sealed class Driver
{
    /// <summary>Уникальный идентификатор водителя, задаётся при регистрации.</summary>
    public int Id { get; }

    /// <summary>Текущее положение водителя на карте.</summary>
    public Location Location { get; internal set; }

    public Driver(int id, Location location)
    {
        Id = id;
        Location = location;
    }

    public override string ToString() => $"Driver #{Id} @ {Location}";
}
