using DriverMatching.Core.Models;

namespace DriverMatching.Core.Algorithms;

/// <summary>
/// Алгоритм 4 — k-d дерево (k = 2: координаты X и Y).
/// Строит сбалансированное дерево поиска по водителям и обходит его,
/// отсекая целые поддеревья, которые заведомо не могут содержать водителя
/// ближе уже найденных <c>count</c> кандидатов.
/// <para>
/// Дерево строится заново при каждом вызове <see cref="FindNearest"/> — это
/// осознанный выбор для предметной области, где водители часто меняют
/// координаты (см. <see cref="DriverRegistry.AddOrUpdateDriver"/>): хранить
/// и инкрементально обновлять одно общее дерево между запросами заметно
/// сложнее и не входит в рамки задания.
/// </para>
/// <para>Сложность: построение — O(D log D), сам обход — в среднем O(log D + count),
/// в худшем случае (вырожденное дерево) — O(D).</para>
/// </summary>
public sealed class KdTreeFinder : INearestDriversFinder
{
    public string Name => "KdTree";

    public IReadOnlyList<DriverDistance> FindNearest(DriverRegistry registry, Location order, int count)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (count <= 0 || registry.DriverCount == 0)
            return Array.Empty<DriverDistance>();

        var items = registry.Drivers.Select(d => (d.Id, d.Location)).ToArray();
        var root = Build(items, 0, items.Length, depth: 0);

        var heap = new PriorityQueue<DriverDistance, double>();
        Search(root, order, count, heap);

        var result = new List<DriverDistance>(heap.Count);
        while (heap.Count > 0)
            result.Add(heap.Dequeue());

        result.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        return result;
    }

    private sealed class Node
    {
        public int DriverId;
        public Location Location;
        public Node? Left;
        public Node? Right;
    }

    /// <summary>Строит сбалансированное k-d дерево, разбивая поочерёдно по X и по Y.</summary>
    private static Node? Build((int Id, Location Location)[] items, int start, int end, int depth)
    {
        int length = end - start;
        if (length <= 0)
            return null;

        int axis = depth % 2; // 0 — делим по X, 1 — делим по Y
        Array.Sort(items, start, length, axis == 0
            ? Comparer<(int Id, Location Location)>.Create((a, b) => a.Location.X.CompareTo(b.Location.X))
            : Comparer<(int Id, Location Location)>.Create((a, b) => a.Location.Y.CompareTo(b.Location.Y)));

        int mid = start + length / 2;
        var node = new Node { DriverId = items[mid].Id, Location = items[mid].Location };
        node.Left = Build(items, start, mid, depth + 1);
        node.Right = Build(items, mid + 1, end, depth + 1);
        return node;
    }

    /// <summary>Рекурсивный обход дерева с поддержанием ограниченной кучи топ-<c>count</c> кандидатов.</summary>
    private static void Search(Node? node, Location target, int count, PriorityQueue<DriverDistance, double> heap, int depth = 0)
    {
        if (node is null)
            return;

        double distance = DistanceUtils.Euclidean(target, node.Location);
        var candidate = new DriverDistance(node.DriverId, node.Location, distance);

        if (heap.Count < count)
        {
            heap.Enqueue(candidate, -distance);
        }
        else if (heap.TryPeek(out _, out double worstNegated) && distance < -worstNegated)
        {
            heap.Dequeue();
            heap.Enqueue(candidate, -distance);
        }

        int axis = depth % 2;
        long diff = axis == 0 ? (long)target.X - node.Location.X : (long)target.Y - node.Location.Y;
        var (near, far) = diff <= 0 ? (node.Left, node.Right) : (node.Right, node.Left);

        Search(near, target, count, heap, depth + 1);

        // В "дальнее" поддерево заходим только если там теоретически может
        // оказаться кандидат ближе, чем худший из уже найденных.
        bool heapFull = heap.Count >= count;
        double worstDistance = heapFull && heap.TryPeek(out _, out double worstNeg)
            ? -worstNeg
            : double.PositiveInfinity;

        if (!heapFull || Math.Abs(diff) < worstDistance)
            Search(far, target, count, heap, depth + 1);
    }
}
