using System.Diagnostics.Contracts;
using System.Numerics;

namespace IslandsLib;

public class Bounds
{
    public Bounds(float minX, float minY, float maxX, float maxY)
    {
        Min = new Vector2(minX, minY);
        Max = new Vector2(maxX, maxY);
    }

    public Vector2 Min { get; set; }
    public Vector2 Max { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is Bounds other && Min.Equals(other.Min) && Max.Equals(other.Max);
    }

    public bool ContainsPoint(Vector2 point)
    {
        return point.Y >= Min.Y && point.Y <= Max.Y && point.X >= Min.X && point.X <= Max.X;
    }

    [Pure]
    public Bounds ExpandBy(float amount)
    {
        return new Bounds(Min.X - amount, Min.Y - amount, Max.X + amount, Max.Y + amount);
    }

    public static Bounds CreateFromSet(IEnumerable<Vector2> set)
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        foreach (var point in set)
        {
            if (point.X < minX) minX = (int)point.X;
            if (point.X > maxX) maxX = (int)point.X;
            if (point.Y < minY) minY = (int)point.Y;
            if (point.Y > maxY) maxY = (int)point.Y;
        }

        return new Bounds(minX: minX, minY: minY, maxX: maxX, maxY: maxY);
    }
}
