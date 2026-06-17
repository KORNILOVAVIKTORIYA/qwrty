using DriverMatching.Core;
using DriverMatching.Core.Exceptions;
using DriverMatching.Core.Models;
using NUnit.Framework;

namespace DriverMatching.Tests;

[TestFixture]
public class DriverRegistryTests
{
    [Test]
    public void AddOrUpdateDriver_NewDriver_StoresLocation()
    {
        var registry = new DriverRegistry(10, 10);

        registry.AddOrUpdateDriver(driverId: 1, x: 3, y: 4);

        Assert.That(registry.GetDriver(1).Location, Is.EqualTo(new Location(3, 4)));
        Assert.That(registry.DriverCount, Is.EqualTo(1));
    }

    [Test]
    public void AddOrUpdateDriver_ExistingDriver_MovesAndFreesOldCell()
    {
        var registry = new DriverRegistry(10, 10);
        registry.AddOrUpdateDriver(1, 0, 0);

        registry.AddOrUpdateDriver(1, 5, 5);

        Assert.That(registry.GetDriver(1).Location, Is.EqualTo(new Location(5, 5)));
        Assert.That(registry.TryGetDriverAt(new Location(0, 0), out _), Is.False);
        Assert.That(registry.DriverCount, Is.EqualTo(1));

        // Старая ячейка освободилась — туда можно поставить другого водителя.
        Assert.DoesNotThrow(() => registry.AddOrUpdateDriver(2, 0, 0));
    }

    [Test]
    public void AddOrUpdateDriver_CellOccupiedByAnotherDriver_Throws()
    {
        var registry = new DriverRegistry(10, 10);
        registry.AddOrUpdateDriver(1, 2, 2);

        Assert.Throws<CellOccupiedException>(() => registry.AddOrUpdateDriver(2, 2, 2));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(10, 0)]
    [TestCase(0, 10)]
    public void AddOrUpdateDriver_OutOfBounds_Throws(int x, int y)
    {
        var registry = new DriverRegistry(10, 10);

        Assert.Throws<OutOfMapBoundsException>(() => registry.AddOrUpdateDriver(1, x, y));
    }

    [Test]
    public void Constructor_NonPositiveSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DriverRegistry(0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DriverRegistry(10, -5));
    }

    [Test]
    public void GetDriver_UnknownId_Throws()
    {
        var registry = new DriverRegistry(10, 10);

        Assert.Throws<DriverNotFoundException>(() => registry.GetDriver(42));
    }

    [Test]
    public void RemoveDriver_ExistingDriver_FreesCellAndReturnsTrue()
    {
        var registry = new DriverRegistry(10, 10);
        registry.AddOrUpdateDriver(1, 1, 1);

        bool removed = registry.RemoveDriver(1);

        Assert.That(removed, Is.True);
        Assert.That(registry.DriverCount, Is.EqualTo(0));
        Assert.That(registry.TryGetDriverAt(new Location(1, 1), out _), Is.False);
    }

    [Test]
    public void RemoveDriver_UnknownId_ReturnsFalse()
    {
        var registry = new DriverRegistry(10, 10);

        Assert.That(registry.RemoveDriver(123), Is.False);
    }
}
