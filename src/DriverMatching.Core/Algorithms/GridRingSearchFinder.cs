using DriverMatching.Core.Models;

namespace DriverMatching.Core.Algorithms;

/// <summary>
/// Алгоритм 3 — поиск «расширяющимися кольцами» вокруг точки заказа.
/// Использует то, что задача описана именно как сетка: вместо перебора всех
/// водителей мы по очереди проверяем границы (кольца) квадратов Чебышёвского
/// радиуса 0, 1, 2, ... вокруг заказа и смотрим, есть ли в соответствующих
/// ячейках водители (через <see cref="DriverRegistry.TryGetDriverAt"/>).
/// <para>
/// Остановка: минимально возможное евклидово расстояние от точки заказа до
/// любой ячейки на кольце радиуса r равно r. Поэтому, как только среди уже
/// найденных кандидатов набралось <c>count</c> штук и расстояние до самого
/// далёкого из них &lt;= r, дальше расширять поиск бессмысленно — ближе уже
/// никого не найти.
/// </para>
/// <para>
/// Сложность зависит от того, насколько близко к заказу реально расположены
/// водители: если на карте размером N*M водителей немного и/или они физически
/// рядом с заказом — алгоритм быстро находит ответ, проверив лишь малую часть
/// карты (около O(r²), где r — итоговый радиус). Если же все водители далеко
/// (или их вообще единицы на огромной карте) — в худшем случае придётся
/// просканировать кольца почти до краёв карты, т.е. до O(N*M).
/// </para>
/// </summary>
public sealed class GridRingSearchFinder : INearestDriversFinder
{
    public string Name => "GridRingSearch";

    public IReadOnlyList<DriverDistance> FindNearest(DriverRegistry registry, Location order, int count)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (count <= 0 || registry.DriverCount == 0)
            return Array.Empty<DriverDistance>();

        var found = new List<DriverDistance>();
        int maxRadius = Math.Max(registry.Width, registry.Height);

        for (int radius = 0; radius <= maxRadius; radius++)
        {
            foreach (var cell in EnumerateRing(order, radius, registry.Width, registry.Height))
            {
                if (registry.TryGetDriverAt(cell, out int driverId))
                    found.Add(new DriverDistance(driverId, cell, DistanceUtils.Euclidean(order, cell)));
            }

            if (found.Count >= count)
            {
                found.Sort((a, b) => a.Distance.CompareTo(b.Distance));

                // Дальше уже не найти ничего ближе count-го кандидата — выходим.
                if (found[count - 1].Distance <= radius)
                    break;

                // Отсекаем лишних "худших" кандидатов, чтобы дальше было меньше сортировать.
                if (found.Count > count)
                    found.RemoveRange(count, found.Count - count);
            }

            if (found.Count == registry.DriverCount)
                break; // нашли вообще всех существующих водителей
        }

        found.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        return found.Count <= count ? found : found.GetRange(0, count);
    }

    /// <summary>
    /// Перечисляет ячейки на границе квадрата Чебышёвского радиуса <paramref name="radius"/>
    /// с центром в <paramref name="center"/>, отсекая ячейки за пределами карты.
    /// </summary>
    private static IEnumerable<Location> EnumerateRing(Location center, int radius, int width, int height)
    {
        if (radius == 0)
        {
            if (IsInside(center, width, height))
                yield return center;
            yield break;
        }

        int minX = center.X - radius, maxX = center.X + radius;
        int minY = center.Y - radius, maxY = center.Y + radius;

        // Верхняя и нижняя стороны квадрата.
        for (int x = minX; x <= maxX; x++)
        {
            var top = new Location(x, minY);
            if (IsInside(top, width, height))
                yield return top;

            var bottom = new Location(x, maxY);
            if (IsInside(bottom, width, height))
                yield return bottom;
        }

        // Левая и правая стороны (без углов — они уже учтены выше).
        for (int y = minY + 1; y <= maxY - 1; y++)
        {
            var left = new Location(minX, y);
            if (IsInside(left, width, height))
                yield return left;

            var right = new Location(maxX, y);
            if (IsInside(right, width, height))
                yield return right;
        }
    }

    private static bool IsInside(Location point, int width, int height) =>
        point.X >= 0 && point.X < width && point.Y >= 0 && point.Y < height;
}
