using DriverMatching.Core.Models;

namespace DriverMatching.Core.Algorithms;

/// <summary>
/// Алгоритм 2 — перебор всех водителей, но без полной сортировки списка.
/// Поддерживаем кучу (<see cref="PriorityQueue{TElement,TPriority}"/>) ограниченного
/// размера <c>count</c>: на её вершине всегда находится «самый плохой» (самый далёкий)
/// из текущих топ-<c>count</c> кандидатов. Если очередной водитель ближе вершины кучи —
/// вершина выбрасывается, а новый кандидат добавляется.
/// <para>Сложность: O(D log count) — заметно быстрее полной сортировки O(D log D)
/// при фиксированном небольшом count (в задаче count = 5), особенно при большом D.</para>
/// </summary>
public sealed class BruteForceHeapFinder : INearestDriversFinder
{
    public string Name => "BruteForceHeap";

    public IReadOnlyList<DriverDistance> FindNearest(DriverRegistry registry, Location order, int count)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (count <= 0)
            return Array.Empty<DriverDistance>();

        // Приоритет = -distance, поэтому "минимум" кучи (вершина) — это
        // максимальное расстояние среди текущих count лучших кандидатов.
        var heap = new PriorityQueue<DriverDistance, double>();

        foreach (var driver in registry.Drivers)
        {
            double distance = DistanceUtils.Euclidean(order, driver.Location);

            if (heap.Count < count)
            {
                heap.Enqueue(new DriverDistance(driver.Id, driver.Location, distance), -distance);
            }
            else if (heap.TryPeek(out _, out double worstNegated) && distance < -worstNegated)
            {
                heap.Dequeue();
                heap.Enqueue(new DriverDistance(driver.Id, driver.Location, distance), -distance);
            }
        }

        var result = new List<DriverDistance>(heap.Count);
        while (heap.Count > 0)
            result.Add(heap.Dequeue());

        result.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        return result;
    }
}
