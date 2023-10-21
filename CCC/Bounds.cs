using System.Numerics;

namespace CCC;

public class Bounds
{
    public Bounds(float minX, float minY, float maxX, float maxY)
    {
        Min = new Vector2(minX, minY);
        Max = new Vector2(maxX, maxY);
    }

    public Vector2 Min { get; set; }
    public Vector2 Max { get; set; }
}
