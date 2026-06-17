using BenchmarkDotNet.Attributes;
using DriverMatching.Core;
using DriverMatching.Core.Algorithms;
using DriverMatching.Core.Models;

namespace DriverMatching.Benchmarks;

/// <summary>
/// Сравнение производительности всех четырёх алгоритмов поиска 5 ближайших
/// к заказу водителей на карте 1000x1000 при разном количестве водителей
/// (100 / 1 000 / 10 000 / 50 000), как требует задание.
/// <para>
/// Запуск (обязательно в конфигурации Release, иначе BenchmarkDotNet выдаст ошибку):
/// <c>dotnet run -c Release --project benchmarks/DriverMatching.Benchmarks</c>
/// </para>
/// </summary>
[MemoryDiagnoser]
public class NearestDriversBenchmarks
{
    private const int Width = 1000;
    private const int Height = 1000;
    private const int RequestedCount = 5;
    private static readonly Location Order = new(Width / 2, Height / 2);

    /// <summary>Число водителей на карте — варьируется BenchmarkDotNet'ом для каждого прогона.</summary>
    [Params(100, 1000, 10000, 50000)]
    public int DriverCount { get; set; }

    private DriverRegistry _registry = null!;

    private readonly BruteForceSortFinder _bruteForceSort = new();
    private readonly BruteForceHeapFinder _bruteForceHeap = new();
    private readonly GridRingSearchFinder _gridRingSearch = new();
    private readonly KdTreeFinder _kdTree = new();

    /// <summary>
    /// Выполняется один раз перед всеми запусками для текущего значения <see cref="DriverCount"/>:
    /// заполняет карту случайными водителями на уникальных ячейках (фиксированный seed — для
    /// воспроизводимости результатов между запусками).
    /// </summary>
    [GlobalSetup]
    public void GlobalSetup()
    {
        _registry = new DriverRegistry(Width, Height);
        var random = new Random(42);
        var used = new HashSet<Location>();
        int placed = 0, id = 1;

        while (placed < DriverCount)
        {
            var loc = new Location(random.Next(Width), random.Next(Height));
            if (used.Add(loc))
            {
                _registry.AddOrUpdateDriver(id++, loc.X, loc.Y);
                placed++;
            }
        }
    }

    [Benchmark(Baseline = true)]
    public IReadOnlyList<DriverDistance> BruteForceSort() =>
        _bruteForceSort.FindNearest(_registry, Order, RequestedCount);

    [Benchmark]
    public IReadOnlyList<DriverDistance> BruteForceHeap() =>
        _bruteForceHeap.FindNearest(_registry, Order, RequestedCount);

    [Benchmark]
    public IReadOnlyList<DriverDistance> GridRingSearch() =>
        _gridRingSearch.FindNearest(_registry, Order, RequestedCount);

    [Benchmark]
    public IReadOnlyList<DriverDistance> KdTree() =>
        _kdTree.FindNearest(_registry, Order, RequestedCount);
}
