using DriverMatching.Core.Models;

namespace DriverMatching.Core.Algorithms;

/// <summary>
/// Общая метрика расстояния, используемая всеми алгоритмами поиска,
/// чтобы их результаты были корректно сопоставимы между собой.
/// </summary>
internal static class DistanceUtils
{
    /// <summary>Евклидово расстояние между двумя ячейками карты.</summary>
    public static double Euclidean(Location a, Location b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
