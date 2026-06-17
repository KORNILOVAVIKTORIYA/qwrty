using DriverMatching.Core.Models;

namespace DriverMatching.Core.Algorithms;

/// <summary>Один результат поиска: водитель, его положение и расстояние до точки заказа.</summary>
public readonly record struct DriverDistance(int DriverId, Location Location, double Distance);
