using DriverMatching.Core.Models;

namespace DriverMatching.Core.Algorithms;

/// <summary>
/// Алгоритм 1 — «грубая сила» (baseline).
/// Считаем расстояние от заказа до КАЖДОГО водителя, полностью сортируем
/// весь список по возрастанию расстояния и берём первые <c>count</c>.
/// <para>Сложность: O(D log D), где D — общее число водителей. Память: O(D).</para>
/// <para>Не зависит от размера карты N*M и от взаимного расположения водителей —
/// поэтому служит надёжной «эталонной» реализацией для проверки остальных алгоритмов.</para>
/// </summary>
public sealed class BruteForceSortFinder : INearestDriversFinder
{
    public string Name => "BruteForceSort";

    public IReadOnlyList<DriverDistance> FindNearest(DriverRegistry registry, Location order, int count)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (count <= 0)
            return Array.Empty<DriverDistance>();

        var result = new List<DriverDistance>(registry.DriverCount);
        foreach (var driver in registry.Drivers)
            result.Add(new DriverDistance(driver.Id, driver.Location, DistanceUtils.Euclidean(order, driver.Location)));

        result.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        return result.Count <= count ? result : result.GetRange(0, count);
    }
}
