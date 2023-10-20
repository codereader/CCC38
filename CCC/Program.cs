﻿using System.Numerics;
using System.Text;

namespace CCC;

public class Program
{
    public static void Main(string[] args)
    {
        //Level1();
        //Level2();
        //Level3();
        //Level4();
        //Level5_6("5");
        //Level5_6("6");
        Level7();

        Console.WriteLine("Done");
    }

    private static List<Vector2> WaterDirections = new List<Vector2>()
    {
        {new Vector2(1, 0) },
        {new Vector2(-1, 0) },
        {new Vector2(0, 1) },
        {new Vector2(0, -1) },
        {new Vector2(1, -1) },
        {new Vector2(1, 1) },
        {new Vector2(-1, 1) },
        {new Vector2(-1, -1) }
    };

    private static List<Vector2> FloodFillDirections = new List<Vector2>()
    {
        {new Vector2(1, 0) },
        {new Vector2(-1, 0) },
        {new Vector2(0, 1) },
        {new Vector2(0, -1) }
    };

    private static void Level7()
    {
        for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
        {
            var inputfilename = $"../../../level7_{inputFileNumber}.in";
            var outputfilename = $"../../../level7_{inputFileNumber}.out";

            Console.WriteLine(inputfilename);

            var lines = File.ReadAllLines(inputfilename).ToList();

            using var outputWriter = new StreamWriter(outputfilename);

            int mapHeight = int.Parse(lines.First());
            var map = lines.Skip(1).Take(mapHeight).ToList();
            var mapWidth = map.First().Length;

            var inputs = lines.Skip(1 + mapHeight + 1).ToList();
            var output = new StringBuilder();

            foreach (var input in inputs)
            {
                var coords1 = input.Split(',').Select(int.Parse).ToArray();
                var startPos = new Vector2(coords1[0], coords1[1]);

                var availablePositions = new Stack<Vector2>();
                availablePositions.Push(startPos);

                var islandPositions = new HashSet<Vector2>();

                var floodFillDirections = FloodFillDirections;

                // flood fill island
                while (availablePositions.Count > 0)
                {
                    var nextPos = availablePositions.Pop();

                    islandPositions.Add(nextPos); // mark as done

                    foreach (var dir in floodFillDirections)
                    {
                        var candidate = nextPos + dir;

                        if (candidate.Y < 0 || candidate.Y >= map.Count ||
                            candidate.X < 0 || candidate.X >= mapWidth)
                        {
                            continue; // out of bounds
                        }

                        if (!islandPositions.Contains(candidate) && map[(int)candidate.Y][(int)candidate.X] == 'L')
                        {
                            availablePositions.Push(candidate);
                        }
                    }
                }

                var minX = islandPositions.Min(p => p.X);
                var maxX = islandPositions.Max(p => p.X);
                var minY = islandPositions.Min(p => p.Y);
                var maxY = islandPositions.Max(p => p.Y);

                var validMinX = minX - 1;
                var validMinY = minY - 1;
                var validMaxX = maxX + 1;
                var validMaxY = maxY + 1;

                var leftEdge = islandPositions.First(p => p.X == minX);
                var firstWaterTile = new Vector2(leftEdge.X - 1, leftEdge.Y);

                if (leftEdge.X < 0) throw new ArgumentOutOfRangeException("Start X out of range");

                // find sea route around the island
                var startRoute = new Route();
                startRoute.AddStartPoint(firstWaterTile);

                var routesToInvestigate = new SortedList<int, HashSet<Route>>();
                routesToInvestigate.Add(startRoute.Length, new HashSet<Route> { startRoute });

                var visitedPoints = new HashSet<Vector2>();
                visitedPoints.Add(firstWaterTile);

                var beachTiles = new HashSet<Vector2>();
                
                foreach (var landTile in islandPositions)
                {
                    foreach (var direction in FloodFillDirections)
                    {
                        var targetPoint = landTile + direction;
                
                        if (map[(int)targetPoint.Y][(int)targetPoint.X] == 'W')
                        {
                            beachTiles.Add(targetPoint);
                        }
                    }
                }

                var winningRoutes = new List<Route>();
                //Route? winningRoute = null;

                var StartingDirections = new List<Vector2>
                {
                    new Vector2(0, +1),
                    new Vector2(+1, +1),
                };

                // First round we only go to the south or south east
                var directions = StartingDirections;

                while (routesToInvestigate.Count > 0)
                {
                    // Pick one of the shortest routes first
                    var shortestRoutes = routesToInvestigate.First().Value;

                    var route = shortestRoutes.First();
                    shortestRoutes.Remove(route);

                    if (shortestRoutes.Count == 0)
                    {
                        routesToInvestigate.Remove(route.Length);
                    }

                    //VisualizeRoute(route, map);

                    var currentPos = route.EndPoint;

                    foreach (var direction in directions)
                    {
                        var targetPoint = currentPos + direction;

                        if (targetPoint.Y < validMinY || targetPoint.Y > validMaxY ||
                            targetPoint.X < validMinX || targetPoint.X > validMaxX)
                        {
                            continue; // out of valid bounds
                        }

                        if (!beachTiles.Contains(targetPoint))
                        {
                            continue;
                        }

                        if (route.Length > 1 && (targetPoint - route.EndPointMinus1).LengthSquared() == 1.0f)
                        {
                            continue; // skip
                        }

                        if (visitedPoints.Contains(targetPoint))
                        {
                            if (targetPoint == firstWaterTile && RouteIsValid(route, validMinX, validMinY, validMaxX, validMaxY) &&
                                !RouteIsEncirclingOtherIsland(route, startPos, islandPositions, map))
                            {
                                // Don't add the starting point to the route
                                winningRoutes.Add(route);
                                //winningRoute = route;
                                routesToInvestigate.Clear();
                                break;
                            }

                            continue;
                        }

                        if (route.ContainsPoint(targetPoint))
                        {
                            continue;
                        }

                        if (map[(int)targetPoint.Y][(int)targetPoint.X] == 'L')
                        {
                            visitedPoints.Add(targetPoint);
                            continue;
                        }

                        // If the route is crossing itself, discard
                        if (route.PointIsCrossing(currentPos, targetPoint))
                        {
                            visitedPoints.Add(targetPoint);
                            continue; // route would cross itself
                        }

                        var newRoute = route.Clone();

                        newRoute.AddPoint(targetPoint);

                        // Skip any route candidates that are longer than a winning candidate
                        if (winningRoutes.Any(r => r.Length < newRoute.Length))
                        {
                            continue;
                        }

                        if (!routesToInvestigate.TryGetValue(newRoute.Length, out var targetRoutes))
                        {
                            targetRoutes = new HashSet<Route>();
                            routesToInvestigate.Add(newRoute.Length, targetRoutes);
                        }
                        targetRoutes.Add(newRoute);
                        //visitedPoints.Add(targetPoint);
                    }

                    directions = WaterDirections; // From now on consider all options
                }

                if (winningRoutes.Count == 0)
                {
                    throw new InvalidOperationException("No solution found");
                }

                //foreach (var candidate in winningRoutes)
                //{
                //    VisualizeRoute(candidate, map);
                //}

                var winningRoute = winningRoutes.OrderBy(r => r.Length).First();
                VisualizeRoute(winningRoute, map);

                // Optimize the winning route for distance
                var optimizedRoute = OptimizeRoute(winningRoute, startPos, map, islandPositions);

                output.AppendLine(winningRoute.ToString());
            }

            outputWriter.Write(output.ToString());
        }
    }

    private static Route OptimizeRoute(Route winningRoute, Vector2 startPos, List<string> map, HashSet<Vector2> islandPositions)
    {
        var route = winningRoute;

        // The points we can no longer move are stored here
        var fixedPoints = new HashSet<Vector2> { route.Points.First() };

        var optimizeFurther = true;

        while (optimizeFurther)
        {
            var points = route.Points.ToList();

            var nonFixedIndex = points.FindIndex(p => !fixedPoints.Contains(p));

            // Move forward until we find a diagonal
            while (nonFixedIndex < points.Count - 1 && (points[nonFixedIndex] - points[nonFixedIndex + 1]).LengthSquared() == 1)
            {
                fixedPoints.Add(points[nonFixedIndex]);
                ++nonFixedIndex;
            }

            if (nonFixedIndex >= points.Count - 2) break; // no more points to optimise

            // non fixed index is now right before a bend

            var routeStem = new Route();

            foreach (var p in points.Take(nonFixedIndex)) // without the starting point itself
            {
                routeStem.AddPoint(p);
            }

            var sourcePoint = points[nonFixedIndex];
            Route? suitableRoute = null;

            // Move back from the target to find the best possible shortcut
            for (var targetIndex = points.Count - 1; targetIndex > nonFixedIndex + 1; --targetIndex)
            {
                var candidate = routeStem.Clone();

                // Assemble a more or less straight line to the target index
                var targetPoint = points[targetIndex];

                var isValid = true;
                
                foreach (var pointOnLine in GetPointsOnLine(targetPoint, sourcePoint))
                {
                    if (map[(int)pointOnLine.Y][(int)pointOnLine.X] == 'L')
                    {
                        isValid = false;
                        break;
                    }

                    candidate.AddPoint(pointOnLine);
                }

                if (!isValid) continue;

                // Fill up the rest of the route
                for (var i = targetIndex + 1; i < points.Count; ++i)
                {
                    candidate.AddPoint(points[i]);
                }

                // We have a shorter route?
                if (candidate.Length >= route.Length || candidate.Length < 4)
                {
                    continue;
                }

                // Is this route valid?
                if (RouteIsEncirclingOtherIsland(candidate, startPos, islandPositions, map))
                {
                    continue;
                }

                suitableRoute = candidate;
                break;
            }

            // We don't vary this point any more in any future round, we already found the opimum for this point
            fixedPoints.Add(points[nonFixedIndex]);

            if (suitableRoute != null)
            {
                route = suitableRoute;
                VisualizeRoute(route, map);
                continue;
            }
        }

        return route;
    }

    private static IEnumerable<Vector2> GetPointsOnLine(Vector2 start, Vector2 end)
    {
        var delta = end - start;

        if (delta.X > delta.Y)
        {
            var numSteps = delta.X;
            for (var i = 0; i < numSteps; ++i)
            {
                var yStep = delta.Y / numSteps;
                yield return new Vector2(start.X + i, start.Y + yStep * i);
            }
        }
        else
        {
            var numSteps = delta.Y;
            for (var i = 0; i < numSteps; ++i)
            {
                var xStep = delta.X / numSteps;
                yield return new Vector2(start.X + xStep * i, start.Y + i);
            }
        }

        yield return end;
    }

    private static void VisualizeRoute(Route winningRoute, List<string> map)
    {
        for (int y = 0; y < map.Count; ++y)
        {
            var line = map[y];

            for (int x = 0; x < line.Length; ++x)
            {
                var pos = new Vector2(x, y);

                if (winningRoute.ContainsPoint(pos))
                {
                    Console.Write('R');
                }
                else if (line[x] == 'W')
                {
                    Console.Write('.');
                }
                else if(line[x] == 'L')
                {
                    Console.Write('L');
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine();
    }

    private static bool RouteIsEncirclingOtherIsland(Route route, Vector2 startPos, HashSet<Vector2> islandPositions, List<string> map)
    {
        var availablePositions = new Stack<Vector2>();
        availablePositions.Push(startPos);

        var visitedPositions = new HashSet<Vector2>();

        foreach (var position in route.Points)
        {
            visitedPositions.Add(position);
        }

        // flood fill island
        while (availablePositions.Count > 0)
        {
            var nextPos = availablePositions.Pop();

            visitedPositions.Add(nextPos); // mark as done

            foreach (var dir in FloodFillDirections)
            {
                var candidate = nextPos + dir;

                if (visitedPositions.Contains(candidate))
                {
                    continue;
                }

                if (map[(int)candidate.Y][(int)candidate.X] == 'L' && 
                    !islandPositions.Contains(candidate))
                {
                    return true;
                }

                availablePositions.Push(candidate);
            }
        }

        return false;
    }


    private static bool RouteIsValid(Route route, float minX, float minY, float maxX, float maxY)
    {
        var routeMinX = int.MaxValue;
        var routeMinY = int.MaxValue;
        var routeMaxX = int.MinValue;
        var routeMaxY = int.MinValue;

        foreach (var point in route.Points)
        {
            if (point.X < routeMinX) routeMinX = (int)point.X;
            if (point.X > routeMaxX) routeMaxX = (int)point.X;
            if (point.Y < routeMinY) routeMinY = (int)point.Y;
            if (point.Y > routeMaxY) routeMaxY = (int)point.Y;
        }

        if (routeMinX == minX && routeMaxX == maxX && routeMaxY == maxY && routeMinY == minY)
        {
            return true;
        }

        return false;
    }

    public class Route
    {
        private HashSet<Vector2> _halfPoints = new();
        private List<Vector2> _points = new();

        public IEnumerable<Vector2> Points => _points;

        public Vector2 EndPoint => _points[_points.Count - 1];
        public Vector2 EndPointMinus1 => _points[_points.Count - 2];

        public int Length => _points.Count;

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

        public override bool Equals(object? obj)
        {
            if (obj is Route other)
            {
                if (other.Length != Length)
                {
                    return false;
                }

                for (int i = 0; i < _points.Count; i++)
                {
                    if (other._points[i] != _points[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int result = 17;

            for (int i = 0; i < _points.Count; i++)
            {
                unchecked
                {
                    result = result * 23 + _points[i].GetHashCode();
                }
            }
            return result;
        }

        public class LengthComparer : IComparer<Route>
        {
            public int Compare(Route? x, Route? y)
            {
                return x.Length.CompareTo(y.Length);
            }
        }
    }


}
