using DriverMatching.Core.Models;

namespace DriverMatching.Core.Algorithms;

/// <summary>
/// Алгоритм поиска <c>count</c> ближайших к заказу водителей.
/// Все реализации этого интерфейса должны возвращать эквивалентный
/// (по расстояниям) результат — отличаться может только то, КАК он получен.
/// </summary>
public interface INearestDriversFinder
{
    /// <summary>Имя алгоритма (используется в тестах и бенчмарках).</summary>
    string Name { get; }

    /// <summary>
    /// Возвращает не более <paramref name="count"/> ближайших к точке <paramref name="order"/>
    /// водителей, отсортированных по возрастанию расстояния.
    /// Если водителей меньше, чем <paramref name="count"/>, возвращаются все имеющиеся.
    /// </summary>
    IReadOnlyList<DriverDistance> FindNearest(DriverRegistry registry, Location order, int count);
}
