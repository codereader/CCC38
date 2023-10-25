using System.Numerics;

namespace IslandsLib;

public class Route
{
    private HashSet<Vector2> _halfPoints = new();
    private List<Vector2> _points = new();

    public IEnumerable<Vector2> Points => _points;

    public Vector2 EndPoint => _points[_points.Count - 1];

    public Vector2 SecondToLastPoint => _points[_points.Count - 2];

    public int Length => _points.Count;

    public float PythagoreanLengthSquared
    {
        get
        {
            if (_points.Count <= 1) return 0;

            var length = 0;

            for (int i = 1; i < _points.Count; i++)
            {
                length += (int)(_points[i - 1] - _points[i]).LengthSquared();
            }

            return length;
        }
    }

    internal void AddStartPoint(Vector2 point)
    {
        _points.Add(point);
        _halfPoints.Add(point);
    }

    internal void AddPoint(Vector2 targetPoint)
    {
        if (_points.Count == 0)
        {
            AddStartPoint(targetPoint);
            return;
        }

        _halfPoints.Add((EndPoint + targetPoint) / 2);
        _points.Add(targetPoint);
    }

    internal bool PointIsCrossing(Vector2 currentPoint, Vector2 targetPoint)
    {
        var halfPoint = (currentPoint + targetPoint) / 2;
        return _halfPoints.Contains(halfPoint);
    }

    internal Route Clone()
    {
        var clone = new Route();

        clone._points = _points.ToList();
        clone._halfPoints = new HashSet<Vector2>(_halfPoints);

        return clone;
    }

    public override string ToString()
    {
        return string.Join(' ', _points.Select(p => $"{(int)p.X},{(int)p.Y}"));
    }

    internal bool ContainsPoint(Vector2 targetPoint)
    {
        return _points.Contains(targetPoint);
    }
}
