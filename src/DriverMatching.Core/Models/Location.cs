namespace DriverMatching.Core.Models;

/// <summary>
/// Координата ячейки на карте размером N*M (0 &lt;= X &lt; N, 0 &lt;= Y &lt; M).
/// Используется как для положения водителя, так и для точки заказа.
/// </summary>
public readonly record struct Location(int X, int Y)
{
    public override string ToString() => $"({X}; {Y})";
}
