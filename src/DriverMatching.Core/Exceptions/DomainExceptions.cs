namespace DriverMatching.Core.Exceptions;

/// <summary>Координаты выходят за границы карты N*M.</summary>
public sealed class OutOfMapBoundsException : Exception
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public OutOfMapBoundsException(int x, int y, int width, int height)
        : base($"Точка ({x}; {y}) выходит за пределы карты {width}x{height}. " +
               $"Допустимый диапазон: 0 <= X < {width}, 0 <= Y < {height}.")
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

/// <summary>В ячейке уже находится другой водитель (в ячейке допускается не более одного).</summary>
public sealed class CellOccupiedException : Exception
{
    public int X { get; }
    public int Y { get; }
    public int OccupyingDriverId { get; }

    public CellOccupiedException(int x, int y, int occupyingDriverId)
        : base($"Ячейка ({x}; {y}) уже занята водителем #{occupyingDriverId}.")
    {
        X = x;
        Y = y;
        OccupyingDriverId = occupyingDriverId;
    }
}

/// <summary>Водитель с указанным идентификатором не зарегистрирован.</summary>
public sealed class DriverNotFoundException : Exception
{
    public int DriverId { get; }

    public DriverNotFoundException(int driverId)
        : base($"Водитель с идентификатором #{driverId} не найден.")
    {
        DriverId = driverId;
    }
}
