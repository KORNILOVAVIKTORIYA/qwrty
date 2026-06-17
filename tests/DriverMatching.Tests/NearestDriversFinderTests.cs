using DriverMatching.Core;
using DriverMatching.Core.Algorithms;
using DriverMatching.Core.Models;
using NUnit.Framework;

namespace DriverMatching.Tests;

/// <summary>
/// Один и тот же набор тестов прогоняется для каждого из четырёх алгоритмов
/// поиска ближайших водителей (см. атрибуты <see cref="TestFixtureAttribute"/> ниже).
/// Это гарантирует, что все реализации <see cref="INearestDriversFinder"/>
/// ведут себя одинаково корректно на одних и тех же входных данных.
/// </summary>
[TestFixture(typeof(BruteForceSortFinder))]
[TestFixture(typeof(BruteForceHeapFinder))]
[TestFixture(typeof(GridRingSearchFinder))]
[TestFixture(typeof(KdTreeFinder))]
public class NearestDriversFinderTests<TFinder> where TFinder : INearestDriversFinder, new()
{
    private TFinder _finder = default!;

    [SetUp]
    public void SetUp() => _finder = new TFinder();

    /// <summary>Заполняет карту случайными водителями на уникальных ячейках (без коллизий).</summary>
    private static DriverRegistry BuildRandomRegistry(int seed, int width, int height, int driverCount)
    {
        var registry = new DriverRegistry(width, height);
        var random = new Random(seed);
        var used = new HashSet<Location>();
        int placed = 0, id = 1;

        while (placed < driverCount)
        {
            var loc = new Location(random.Next(width), random.Next(height));
            if (used.Add(loc))
            {
                registry.AddOrUpdateDriver(id++, loc.X, loc.Y);
                placed++;
            }
        }

        return registry;
    }

    [Test]
    public void FindNearest_EmptyRegistry_ReturnsEmptyResult()
    {
        var registry = new DriverRegistry(50, 50);

        var result = _finder.FindNearest(registry, new Location(10, 10), 5);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void FindNearest_FewerDriversThanRequested_ReturnsAllOfThem()
    {
        var registry = new DriverRegistry(50, 50);
        registry.AddOrUpdateDriver(1, 0, 0);
        registry.AddOrUpdateDriver(2, 1, 1);

        var result = _finder.FindNearest(registry, new Location(0, 0), 5);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void FindNearest_CountIsZero_ReturnsEmptyResult()
    {
        var registry = new DriverRegistry(50, 50);
        registry.AddOrUpdateDriver(1, 0, 0);

        var result = _finder.FindNearest(registry, new Location(0, 0), 0);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void FindNearest_OrderExactlyOnDriverCell_ReturnsThatDriverWithZeroDistanceFirst()
    {
        var registry = new DriverRegistry(50, 50);
        registry.AddOrUpdateDriver(1, 25, 25);
        registry.AddOrUpdateDriver(2, 0, 0);
        registry.AddOrUpdateDriver(3, 49, 49);

        var result = _finder.FindNearest(registry, new Location(25, 25), 5);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result[0].DriverId, Is.EqualTo(1));
        Assert.That(result[0].Distance, Is.EqualTo(0.0).Within(1e-9));
    }

    [Test]
    public void FindNearest_ResultIsSortedAscendingByDistance()
    {
        var registry = BuildRandomRegistry(seed: 11, width: 300, height: 300, driverCount: 40);

        var result = _finder.FindNearest(registry, new Location(150, 150), 5);

        Assert.That(result.Count, Is.EqualTo(5));
        for (int i = 1; i < result.Count; i++)
            Assert.That(result[i].Distance, Is.GreaterThanOrEqualTo(result[i - 1].Distance));
    }

    [Test]
    public void FindNearest_MoreDriversThanRequested_ReturnsExactlyCount()
    {
        var registry = BuildRandomRegistry(seed: 7, width: 100, height: 100, driverCount: 30);

        var result = _finder.FindNearest(registry, new Location(50, 50), 5);

        Assert.That(result.Count, Is.EqualTo(5));
    }

    [Test]
    public void FindNearest_DriversAtMapCorners_FindsClosestDriverNearOrder()
    {
        var registry = new DriverRegistry(20, 20);
        registry.AddOrUpdateDriver(1, 0, 0);
        registry.AddOrUpdateDriver(2, 19, 0);
        registry.AddOrUpdateDriver(3, 0, 19);
        registry.AddOrUpdateDriver(4, 19, 19);
        registry.AddOrUpdateDriver(5, 10, 10);
        registry.AddOrUpdateDriver(6, 9, 9);

        var result = _finder.FindNearest(registry, new Location(10, 10), 5);

        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result.Any(r => r.DriverId == 5), Is.True);
    }

    [Test]
    public void FindNearest_NarrowOneColumnMap_ReturnsDriversInCorrectOrder()
    {
        var registry = new DriverRegistry(1, 10);
        for (int y = 0; y < 10; y++)
            registry.AddOrUpdateDriver(y + 1, 0, y);

        var result = _finder.FindNearest(registry, new Location(0, 0), 5);
        var ids = result.Select(r => r.DriverId).ToArray();

        Assert.That(ids, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }

    /// <summary>
    /// Кросс-валидация: на 25 разных случайных сценариях (разный размер карты,
    /// число водителей и точка заказа) результат алгоритма должен совпадать
    /// с эталонным <see cref="BruteForceSortFinder"/> — по расстояниям, округлённым
    /// до 6 знаков (сравнение именно расстояний, а не ID водителей, специально
    /// исключает ложные срабатывания из-за разного порядка при равных расстояниях).
    /// </summary>
    [Test]
    public void FindNearest_MatchesBruteForceSortGroundTruth_AcrossRandomScenarios()
    {
        for (int seed = 0; seed < 25; seed++)
        {
            int width = 50 + seed * 7;
            int height = 40 + seed * 5;
            int driverCount = Math.Min(5 + seed * 13, width * height - 1);

            var registry = BuildRandomRegistry(seed, width, height, driverCount);
            var order = new Location(seed % width, (seed * 3) % height);

            var groundTruth = new BruteForceSortFinder().FindNearest(registry, order, 5)
                .Select(r => Math.Round(r.Distance, 6)).ToArray();
            var actual = _finder.FindNearest(registry, order, 5)
                .Select(r => Math.Round(r.Distance, 6)).ToArray();

            Assert.That(actual, Is.EqualTo(groundTruth), $"Несовпадение с эталоном при seed={seed}");
        }
    }
}
