using System.Data;
using System.Numerics;
using System.Text;

namespace IslandsLib;

public class Navigator
{
    private static List<Vector2> SeaRouteDirections = new List<Vector2>()
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

    private static List<Vector2> OrthogonalDirections = new List<Vector2>()
{
    {new Vector2(1, 0) },
    {new Vector2(-1, 0) },
    {new Vector2(0, 1) },
    {new Vector2(0, -1) }
};

    private static List<Vector2> SouthAndSouthEast = new List<Vector2>
{
    new Vector2(0, +1),
    new Vector2(+1, +1),
};

    public void Level7()
    {
        foreach (var scenario in GetScenariosForLevel(7))
        {
            Console.WriteLine(scenario.InputFilename);

            using var outputWriter = new StreamWriter(scenario.OutputFilename);

            var output = new StringBuilder();

            foreach (var input in scenario.InputLines)
            {
                var startPos = ParseVector2(input);

                var islandPositions = FloodFillIsland(scenario.Map, startPos);

                var islandBounds = Bounds.CreateFromSet(islandPositions);
                var validBounds = islandBounds.ExpandBy(1);

                var leftEdge = islandPositions.First(p => p.X == islandBounds.Min.X);
                var firstWaterTile = new Vector2(leftEdge.X - 1, leftEdge.Y);

                if (leftEdge.X < 0) throw new ArgumentOutOfRangeException("Start X out of range");

                // find a tight sea route around the island
                var startRoute = new Route();
                startRoute.AddStartPoint(firstWaterTile);

                var routesToInvestigate = new SortedList<int, List<Route>>();
                routesToInvestigate.Add(startRoute.Length, new List<Route> { startRoute });

                var visitedPoints = new HashSet<Vector2>();
                visitedPoints.Add(firstWaterTile);

                // We only consider beach tiles for the tight route around the island
                var beachTiles = new HashSet<Vector2>();

                foreach (var landTile in islandPositions)
                {
                    foreach (var direction in OrthogonalDirections)
                    {
                        var targetPoint = landTile + direction;

                        if (IsWater(scenario.Map, targetPoint))
                        {
                            beachTiles.Add(targetPoint);
                        }
                    }
                }

                Route? winningRoute = null;

                // First round we only go to the south or south east
                var directions = SouthAndSouthEast;

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

                        if (!validBounds.ContainsPoint(targetPoint))
                        {
                            continue; // out of valid bounds
                        }

                        if (!beachTiles.Contains(targetPoint))
                        {
                            continue;
                        }

                        if (route.Length > 1 && (targetPoint - route.SecondToLastPoint).LengthSquared() == 1.0f)
                        {
                            continue; // skip
                        }

                        if (visitedPoints.Contains(targetPoint))
                        {
                            if (targetPoint == firstWaterTile && RouteIsValid(route, validBounds) &&
                                !RouteIsEncirclingOtherIsland(route, startPos, islandPositions, scenario.Map))
                            {
                                // Don't add the starting point to the route
                                winningRoute = route;
                                routesToInvestigate.Clear();
                                break;
                            }

                            continue;
                        }

                        if (route.ContainsPoint(targetPoint))
                        {
                            continue;
                        }

                        if (IsLand(scenario.Map, targetPoint))
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

                        if (!routesToInvestigate.TryGetValue(newRoute.Length, out var targetRoutes))
                        {
                            targetRoutes = new List<Route>();
                            routesToInvestigate.Add(newRoute.Length, targetRoutes);
                        }
                        targetRoutes.Add(newRoute);
                    }

                    directions = SeaRouteDirections; // From now on consider all options
                }

                if (winningRoute == null)
                {
                    throw new InvalidOperationException("No solution found");
                }

                // Optimize the winning route for distance
                var optimizedRoute = OptimizeRoute(winningRoute, startPos, scenario.Map, islandPositions, validBounds);
                //VisualizeRoute(optimizedRoute, map, optimizedRoute.Points);

                output.AppendLine(winningRoute.ToString());
            }

            outputWriter.Write(output.ToString());
        }
    }

    public void Level5_6(int level)
    {
        foreach (var scenario in GetScenariosForLevel(level))
        {
            Console.WriteLine(scenario.InputFilename);

            using var outputWriter = new StreamWriter(scenario.OutputFilename);

            var output = new StringBuilder();

            foreach (var input in scenario.InputLines)
            {
                var startPos = ParseVector2(input);

                var islandPositions = FloodFillIsland(scenario.Map, startPos);

                var islandBounds = Bounds.CreateFromSet(islandPositions);
                var validBounds = islandBounds.ExpandBy(1);

                var leftEdge = islandPositions.First(p => p.X == islandBounds.Min.X);
                var firstWaterTile = new Vector2(leftEdge.X - 1, leftEdge.Y);

                if (leftEdge.X < 0) throw new ArgumentOutOfRangeException("Start X out of range");

                // find sea route around the island
                var startRoute = new Route();
                startRoute.AddStartPoint(firstWaterTile);

                var routesToInvestigate = new SortedList<int, List<Route>>();
                routesToInvestigate.Add(startRoute.Length, new List<Route> { startRoute });

                var visitedPoints = new HashSet<Vector2>();
                visitedPoints.Add(firstWaterTile);

                Route? winningRoute = null;

                // First round we only go to the south or south east
                var directions = SouthAndSouthEast;

                while (routesToInvestigate.Count > 0)
                {
                    // Pick one of the shortest routes first
                    var shortestRoutes = routesToInvestigate.First().Value;

                    var route = shortestRoutes[shortestRoutes.Count - 1];
                    shortestRoutes.RemoveAt(shortestRoutes.Count - 1);

                    if (shortestRoutes.Count == 0)
                    {
                        routesToInvestigate.Remove(route.Length);
                    }

                    var currentPos = route.EndPoint;

                    foreach (var direction in directions)
                    {
                        var targetPoint = currentPos + direction;

                        if (!validBounds.ContainsPoint(targetPoint))
                        {
                            continue; // out of valid bounds
                        }

                        if (visitedPoints.Contains(targetPoint))
                        {
                            if (targetPoint == firstWaterTile && RouteIsValid(route, validBounds))
                            {
                                // Don't add the starting point to the route
                                winningRoute = route;
                                routesToInvestigate.Clear();
                                break;
                            }

                            continue;
                        }

                        if (IsLand(scenario.Map, targetPoint))
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

                        if (!routesToInvestigate.TryGetValue(newRoute.Length, out var targetRoutes))
                        {
                            targetRoutes = new List<Route>();
                            routesToInvestigate.Add(newRoute.Length, targetRoutes);
                        }
                        targetRoutes.Add(newRoute);
                        visitedPoints.Add(targetPoint);
                    }

                    directions = SeaRouteDirections; // From now on consider all options
                }

                if (winningRoute == null)
                {
                    throw new InvalidOperationException("No solution found");
                }

                output.AppendLine(winningRoute.ToString());
            }

            outputWriter.Write(output.ToString());
        }
    }

    public void Level4()
    {
        foreach (var scenario in GetScenariosForLevel(4))
        {
            Console.WriteLine(scenario.InputFilename);

            using var outputWriter = new StreamWriter(scenario.OutputFilename);

            var output = new StringBuilder();

            foreach (var input in scenario.InputLines)
            {
                var (startPos, endPos) = ParseCoordinatePair(input);

                var startRoute = new Route();
                startRoute.AddStartPoint(startPos);

                var visitedPoints = new HashSet<Vector2>();

                var routesToInvestigate = new SortedList<int, List<Route>>();
                routesToInvestigate.Add(startRoute.Length, new List<Route> { startRoute });

                var directions = SeaRouteDirections;

                Route? winningRoute = null;

                while (routesToInvestigate.Count > 0)
                {
                    // Pick one of the shortest routes first
                    var shortestRoutes = routesToInvestigate.First().Value;

                    var route = shortestRoutes[shortestRoutes.Count - 1];
                    shortestRoutes.RemoveAt(shortestRoutes.Count - 1);

                    if (shortestRoutes.Count == 0)
                    {
                        routesToInvestigate.Remove(route.Length);
                    }

                    var currentPos = route.EndPoint;

                    foreach (var direction in directions)
                    {
                        var targetPoint = currentPos + direction;

                        if (!scenario.MapBounds.ContainsPoint(targetPoint))
                        {
                            continue; // out of bounds
                        }

                        if (visitedPoints.Contains(targetPoint))
                        {
                            continue;
                        }

                        if (targetPoint == endPos)
                        {
                            route.AddPoint(targetPoint);
                            winningRoute = route;
                            routesToInvestigate.Clear();
                            break;
                        }

                        if (IsLand(scenario.Map, targetPoint))
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

                        if (!routesToInvestigate.TryGetValue(newRoute.Length, out var targetRoutes))
                        {
                            targetRoutes = new List<Route>();
                            routesToInvestigate.Add(newRoute.Length, targetRoutes);
                        }
                        targetRoutes.Add(newRoute);
                        visitedPoints.Add(targetPoint);
                    }
                }

                if (winningRoute == null)
                {
                    throw new InvalidOperationException("No solution found");
                }

                output.AppendLine(winningRoute.ToString());
            }

            outputWriter.Write(output.ToString());
        }
    }

    public void Level3()
    {
        foreach (var scenario in GetScenariosForLevel(3))
        {
            Console.WriteLine(scenario.InputFilename);

            using var outputWriter = new StreamWriter(scenario.OutputFilename);

            var output = new StringBuilder();

            foreach (var input in scenario.InputLines)
            {
                var numberPairs = input.Split(' ').ToArray();

                var visitedPositions = new HashSet<Vector2>();
                var intersectsWithItself = false;

                var startPos = ParseVector2(numberPairs.First());
                visitedPositions.Add(startPos);

                var lastPos = startPos;

                foreach (var coordPair in numberPairs.Skip(1))
                {
                    var pos = ParseVector2(coordPair);

                    if (visitedPositions.Contains(pos))
                    {
                        // Direct hit
                        intersectsWithItself = true;
                        break;
                    }

                    var halfPos = (pos + lastPos) / 2;

                    if (visitedPositions.Contains(halfPos))
                    {
                        // Halfpos hit
                        intersectsWithItself = true;
                        break;
                    }

                    visitedPositions.Add(halfPos);
                    visitedPositions.Add(pos);

                    lastPos = pos;
                }

                output.AppendLine(intersectsWithItself ? "INVALID" : "VALID");
            }

            outputWriter.Write(output.ToString());
        }
    }

    public void Level2()
    {
        foreach (var scenario in GetScenariosForLevel(2))
        {
            Console.WriteLine(scenario.InputFilename);

            using var outputWriter = new StreamWriter(scenario.OutputFilename);

            var output = new StringBuilder();

            foreach (var input in scenario.InputLines)
            {
                var (pos1, pos2) = ParseCoordinatePair(input);

                var availablePositions = new Stack<Vector2>();
                availablePositions.Push(pos1);

                var visitedPositions = new HashSet<Vector2>();

                bool isOnSameIsland = false;

                var directions = OrthogonalDirections;

                while (availablePositions.Count > 0)
                {
                    var nextPos = availablePositions.Pop();

                    visitedPositions.Add(nextPos); // mark as done

                    if (nextPos == pos2)
                    {
                        isOnSameIsland = true;
                        break;
                    }

                    foreach (var dir in directions)
                    {
                        var candidate = nextPos + dir;

                        if (!scenario.MapBounds.ContainsPoint(candidate))
                        {
                            continue; // out of bounds
                        }

                        if (!visitedPositions.Contains(candidate) && IsLand(scenario.Map, candidate))
                        {
                            availablePositions.Push(candidate);
                        }
                    }
                }

                output.AppendLine(isOnSameIsland ? "SAME" : "DIFFERENT");
            }

            outputWriter.Write(output.ToString());
        }
    }

    public void Level1()
    {
        foreach (var scenario in GetScenariosForLevel(1))
        {
            Console.WriteLine(scenario.InputFilename);

            using var outputWriter = new StreamWriter(scenario.OutputFilename);

            var output = new StringBuilder();

            foreach (var input in scenario.InputLines)
            {
                var pos = ParseVector2(input);

                output.Append(scenario.Map[(int)pos.Y][(int)pos.X]);
                output.AppendLine();
            }

            outputWriter.Write(output.ToString());
        }
    }


    private static Route OptimizeRoute(Route winningRoute, Vector2 startPos, List<string> map, HashSet<Vector2> islandPositions, Bounds validBounds)
    {
        var route = winningRoute;

        // The points we can no longer move are stored here
        var fixedPoints = new HashSet<Vector2>();

        var optimizeFurther = true;

        while (optimizeFurther)
        {
            var points = route.Points.ToList();

            //Console.WriteLine("Starting Situation");
            //VisualizeRoute(route, map, fixedPoints);

            var nonFixedIndex = points.FindIndex(p => !fixedPoints.Contains(p));

            if (nonFixedIndex >= points.Count - 2) break; // no more points to optimise

            var routeStem = new Route();

            foreach (var p in points.Take(nonFixedIndex)) // without the starting point itself
            {
                routeStem.AddPoint(p);
            }

            var sourcePoint = points[nonFixedIndex];
            Route? suitableRoute = null;

            // Move back from the end of the route to find the shortcut cutting of the most
            for (var targetIndex = points.Count - 1; targetIndex > nonFixedIndex + 1; --targetIndex)
            {
                var candidate = routeStem.Clone();

                // Assemble a (rasterized) straight line to the target index
                var targetPoint = points[targetIndex];

                var isValid = true;

                foreach (var pointOnLine in GetPointsOnLine(sourcePoint, targetPoint))
                {
                    if (IsLand(map, pointOnLine) || candidate.ContainsPoint(pointOnLine))
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

                // Did we find a shorter route?
                if (candidate.Length < 4 || candidate.PythagoreanLengthSquared >= route.PythagoreanLengthSquared)
                {
                    continue;
                }

                // Check route validity
                if (!RouteIsValid(candidate, validBounds) ||
                    RouteIsEncirclingOtherIsland(candidate, startPos, islandPositions, map))
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
                //VisualizeRoute(route, map, fixedPoints);
                continue;
            }
        }

        return route;
    }

    /// <summary>
    /// Return the rasterized line from start to end, including the start / end points themselves.
    /// </summary>
    private static IEnumerable<Vector2> GetPointsOnLine(Vector2 start, Vector2 end)
    {
        var delta = end - start;

        if (Math.Abs(delta.X) > Math.Abs(delta.Y))
        {
            var numSteps = Math.Abs(delta.X);
            var deltaX = delta.X < 1 ? -1 : 1;
            for (var i = 0; i < numSteps; ++i)
            {
                var yStep = delta.Y / numSteps;
                yield return new Vector2(start.X + deltaX * i, (int)(start.Y + yStep * i));
            }
        }
        else
        {
            var numSteps = Math.Abs(delta.Y);
            var deltaY = delta.Y < 1 ? -1 : 1;
            for (var i = 0; i < numSteps; ++i)
            {
                var xStep = delta.X / numSteps;
                yield return new Vector2((int)(start.X + xStep * i), start.Y + deltaY * i);
            }
        }

        yield return end;
    }

    private static void VisualizeRoute(Route route, List<string> map, IEnumerable<Vector2>? highlightPoints = null)
    {
        var highlights = highlightPoints?.ToList() ?? new List<Vector2>();
        var defaultColour = Console.ForegroundColor;

        for (int y = 0; y < map.Count; ++y)
        {
            var line = map[y];

            for (int x = 0; x < line.Length; ++x)
            {
                var pos = new Vector2(x, y);

                Console.ForegroundColor = highlights.Contains(pos) ? ConsoleColor.Red : defaultColour;

                if (route.ContainsPoint(pos))
                {
                    Console.Write('R');
                }
                else if (line[x] == 'W')
                {
                    Console.Write('.');
                }
                else if (line[x] == 'L')
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

        var visitedPositions = new HashSet<Vector2>(route.Points);

        // Flood fill, including water tiles, breaking on any foreign island
        while (availablePositions.Count > 0)
        {
            var nextPos = availablePositions.Pop();

            visitedPositions.Add(nextPos); // mark as done

            foreach (var dir in OrthogonalDirections)
            {
                var candidate = nextPos + dir;

                if (visitedPositions.Contains(candidate))
                {
                    continue;
                }

                if (IsLand(map, candidate) && !islandPositions.Contains(candidate))
                {
                    return true;
                }

                availablePositions.Push(candidate);
            }
        }

        return false;
    }
    private static bool RouteIsValid(Route route, Bounds validBounds)
    {
        var routeBounds = Bounds.CreateFromSet(route.Points);
        return routeBounds.Equals(validBounds);
    }


    private static bool IsLand(IList<string> map, Vector2 position)
    {
        return map[(int)position.Y][(int)position.X] == 'L';
    }

    private static bool IsWater(IList<string> map, Vector2 position)
    {
        return map[(int)position.Y][(int)position.X] == 'W';
    }

    private static HashSet<Vector2> FloodFillIsland(List<string> map, Vector2 startPos)
    {
        var islandPositions = new HashSet<Vector2>();

        var mapBounds = new Bounds(0, 0, map[0].Length - 1, map.Count - 1);

        var availablePositions = new Stack<Vector2>();
        availablePositions.Push(startPos);

        var floodFillDirections = OrthogonalDirections;

        // Find all land positions while we have any points to investigate
        while (availablePositions.Count > 0)
        {
            var nextPos = availablePositions.Pop();

            islandPositions.Add(nextPos); // mark as done

            foreach (var dir in floodFillDirections)
            {
                var candidate = nextPos + dir;

                if (!mapBounds.ContainsPoint(candidate))
                {
                    continue; // out of bounds
                }

                if (IsLand(map, candidate) && !islandPositions.Contains(candidate))
                {
                    availablePositions.Push(candidate);
                }
            }
        }

        return islandPositions;
    }



    private class Scenario
    {
        public Scenario(int level, int stage)
        {
            InputFilename = $"../../../level{level}_{stage}.in";
            OutputFilename = Path.ChangeExtension(InputFilename, ".out");
            Lines = File.ReadAllLines(InputFilename).ToList();

            MapHeight = int.Parse(Lines.First());
            Map = Lines.Skip(1).Take(MapHeight).ToList();
            MapWidth = Map.First().Length;
            MapBounds = new Bounds(0, 0, MapWidth - 1, MapHeight - 1);

            InputLines = Lines.Skip(1 + MapHeight + 1).ToList();
        }

        public string InputFilename { get; }
        public string OutputFilename { get; }
        public List<string> Lines { get; }

        /// <summary>
        /// The whole map, each row is a string with 'L' being Land, and 'W' being water
        /// </summary>
        public List<string> Map { get; }

        public int MapWidth { get; }
        public int MapHeight { get; }

        public Bounds MapBounds { get; }

        public List<string> InputLines { get; }
    }
    private static IEnumerable<Scenario> GetScenariosForLevel(int level) => Enumerable.Range(1, 5).Select(stage => new Scenario(level, stage));

    /// <summary>
    /// Parse a single input coordinate pair like "56,89" into a Vector2
    /// </summary>
    private static Vector2 ParseVector2(string input)
    {
        var coords = input.Split(',').Select(int.Parse).ToArray();
        return new Vector2(coords[0], coords[1]);
    }

    /// <summary>
    /// Parse a coordinate pair from a string, like "5,78 9,79"
    /// </summary>
    private static (Vector2, Vector2) ParseCoordinatePair(string input)
    {
        var pairs = input.Split(' ').ToArray();
        return (ParseVector2(pairs[0]), ParseVector2(pairs[1]));
    }

}