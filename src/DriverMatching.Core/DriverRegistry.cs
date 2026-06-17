using DriverMatching.Core.Exceptions;
using DriverMatching.Core.Models;

namespace DriverMatching.Core;

/// <summary>
/// Хранилище положений водителей на прямоугольной карте Width*Height,
/// состоящей из ячеек 1*1. В каждой ячейке допускается не более одного водителя.
/// </summary>
public sealed class DriverRegistry
{
    private readonly Dictionary<int, Driver> _driversById = new();
    private readonly Dictionary<Location, int> _driverIdByLocation = new();

    /// <summary>Размер карты по оси X (N). Допустимые координаты X: [0; Width).</summary>
    public int Width { get; }

    /// <summary>Размер карты по оси Y (M). Допустимые координаты Y: [0; Height).</summary>
    public int Height { get; }

    public DriverRegistry(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Ширина карты должна быть положительной.");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Высота карты должна быть положительной.");

        Width = width;
        Height = height;
    }

    /// <summary>Текущее количество зарегистрированных водителей.</summary>
    public int DriverCount => _driversById.Count;

    /// <summary>Снимок всех водителей на текущий момент.</summary>
    public IReadOnlyCollection<Driver> Drivers => _driversById.Values;

    /// <summary>
    /// Регистрирует нового водителя с координатами (x, y) либо, если водитель
    /// с таким <paramref name="driverId"/> уже существует, переносит его на новые координаты.
    /// </summary>
    /// <exception cref="OutOfMapBoundsException">Координаты выходят за пределы карты.</exception>
    /// <exception cref="CellOccupiedException">Целевая ячейка уже занята другим водителем.</exception>
    public void AddOrUpdateDriver(int driverId, int x, int y)
    {
        EnsureInsideMap(x, y);
        var newLocation = new Location(x, y);

        if (_driverIdByLocation.TryGetValue(newLocation, out int occupantId) && occupantId != driverId)
            throw new CellOccupiedException(x, y, occupantId);

        if (_driversById.TryGetValue(driverId, out var driver))
        {
            _driverIdByLocation.Remove(driver.Location);
            driver.Location = newLocation;
        }
        else
        {
            driver = new Driver(driverId, newLocation);
            _driversById.Add(driverId, driver);
        }

        _driverIdByLocation[newLocation] = driverId;
    }

    /// <summary>Удаляет водителя из реестра. Возвращает false, если водитель не найден.</summary>
    public bool RemoveDriver(int driverId)
    {
        if (!_driversById.TryGetValue(driverId, out var driver))
            return false;

        _driversById.Remove(driverId);
        _driverIdByLocation.Remove(driver.Location);
        return true;
    }

    /// <exception cref="DriverNotFoundException">Водитель не найден.</exception>
    public Driver GetDriver(int driverId)
    {
        if (!_driversById.TryGetValue(driverId, out var driver))
            throw new DriverNotFoundException(driverId);

        return driver;
    }

    /// <summary>Проверяет, занята ли ячейка, и возвращает идентификатор находящегося там водителя.</summary>
    public bool TryGetDriverAt(Location location, out int driverId) =>
        _driverIdByLocation.TryGetValue(location, out driverId);

    private void EnsureInsideMap(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new OutOfMapBoundsException(x, y, Width, Height);
    }
}
