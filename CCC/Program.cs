using System.Numerics;
using System.Text;

namespace CCC;

public class Program
{
    public static void Main(string[] args)
    {
        Level1();
        Level2();
        Level3();
        Level4();
        Level5_6("5");
        Level5_6("6");
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

                var validBounds = new Bounds(minX: minX - 1, minY: minY - 1, maxX: maxX + 1, maxY: maxY + 1);

                var leftEdge = islandPositions.First(p => p.X == minX);
                var firstWaterTile = new Vector2(leftEdge.X - 1, leftEdge.Y);

                if (leftEdge.X < 0) throw new ArgumentOutOfRangeException("Start X out of range");

                // find sea route around the island
                var startRoute = new Route();
                startRoute.AddStartPoint(firstWaterTile);

                var routesToInvestigate = new SortedList<int, List<Route>>();
                routesToInvestigate.Add(startRoute.Length, new List<Route> { startRoute });

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

                        if (targetPoint.Y < validBounds.Min.Y || targetPoint.Y > validBounds.Max.Y ||
                            targetPoint.X < validBounds.Min.X || targetPoint.X > validBounds.Max.X)
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
                            if (targetPoint == firstWaterTile && RouteIsValid(route, validBounds) &&
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
                            targetRoutes = new List<Route>();
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

                // Optimize the winning route for distance
                var optimizedRoute = OptimizeRoute(winningRoute, startPos, map, islandPositions, validBounds);
                //VisualizeRoute(optimizedRoute, map, optimizedRoute.Points);

                output.AppendLine(winningRoute.ToString());
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

            var nonFixedIndex = fixedPoints.Count == 0 ? 0 : points.FindIndex(p => !fixedPoints.Contains(p));

            // Move forward until we find a diagonal
            //while (nonFixedIndex < points.Count - 1 && (points[nonFixedIndex] - points[nonFixedIndex + 1]).LengthSquared() == 1)
            //{
            //    fixedPoints.Add(points[nonFixedIndex]);
            //    ++nonFixedIndex;
            //}

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

                var line = GetPointsOnLine(sourcePoint, targetPoint).ToList();

                foreach (var pointOnLine in line)
                {
                    if (map[(int)pointOnLine.Y][(int)pointOnLine.X] == 'L' ||
                        candidate.ContainsPoint(pointOnLine))
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
                if (candidate.PyLengthSquared >= route.PyLengthSquared || candidate.Length < 4)
                {
                    continue;
                }

                //Console.WriteLine("Candidate:\n{0}", candidate.ToString());
                //VisualizeRoute(candidate, map, fixedPoints.Concat(new[] { targetPoint }));

                // Check route validity
                if (!RouteIsValid(candidate, validBounds) ||
                    RouteIsEncirclingOtherIsland(candidate, startPos, islandPositions, map))
                {
                    continue;
                }

                // Mark the whole shortcut as fixed
                //foreach (var point in line)
                //{
                //    fixedPoints.Add(point);
                //}

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

        //Console.WriteLine("Optimised Route:");
        //VisualizeRoute(route, map, route.Points);

        return route;
    }

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

    private static void VisualizeRoute(Route route, List<string> map, IEnumerable<Vector2> highlightPoints = null)
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


    private static bool RouteIsValid(Route route, Bounds validBounds)
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

        if (routeMinX == validBounds.Min.X && routeMaxX == validBounds.Max.X &&
            routeMaxY == validBounds.Max.Y && routeMinY == validBounds.Min.Y)
        {
            return true;
        }

        return false;
    }

    private static void Level5_6(string level)
    {
        for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
        {
            var inputfilename = $"../../../level{level}_{inputFileNumber}.in";
            var outputfilename = $"../../../level{level}_{inputFileNumber}.out";

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

                var floodFillDirections = new List<Vector2>()
                {
                    {new Vector2(1, 0) },
                    {new Vector2(-1, 0) },
                    {new Vector2(0, 1) },
                    {new Vector2(0, -1) }
                };

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

                var validBounds = new Bounds(minX: minX - 1, minY: minY - 1, maxX: maxX + 1, maxY: maxY + 1);

                var leftEdge = islandPositions.First(p => p.X == minX);
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

                        if (targetPoint.Y < validBounds.Min.Y || targetPoint.Y > validBounds.Max.Y ||
                            targetPoint.X < validBounds.Min.X || targetPoint.X > validBounds.Max.X)
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

                        if (!routesToInvestigate.TryGetValue(newRoute.Length, out var targetRoutes))
                        {
                            targetRoutes = new List<Route>();
                            routesToInvestigate.Add(newRoute.Length, targetRoutes);
                        }
                        targetRoutes.Add(newRoute);
                        visitedPoints.Add(targetPoint);
                    }

                    directions = WaterDirections; // From now on consider all options
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

    private static void Level4()
    {
        for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
        {
            var inputfilename = $"../../../level4_{inputFileNumber}.in";
            var outputfilename = $"../../../level4_{inputFileNumber}.out";

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
                var coordPair = input.Split(' ').ToArray();
                var coords1 = coordPair[0].Split(',').Select(int.Parse).ToArray();
                var startPos = new Vector2(coords1[0], coords1[1]);

                var coords2 = coordPair[1].Split(',').Select(int.Parse).ToArray();
                var endPos = new Vector2(coords2[0], coords2[1]);

                var startRoute = new Route();
                startRoute.AddStartPoint(startPos);

                var visitedPoints = new HashSet<Vector2>();

                var routesToInvestigate = new SortedList<int, List<Route>>();

                routesToInvestigate.Add(startRoute.Length, new List<Route> { startRoute });

                var directions = WaterDirections;

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

                        if (targetPoint.Y < 0 || targetPoint.Y >= map.Count ||
                            targetPoint.X < 0 || targetPoint.X >= mapWidth)
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

    private static void Level3()
    {
        for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
        {
            var inputfilename = $"../../../level3_{inputFileNumber}.in";
            var outputfilename = $"../../../level3_{inputFileNumber}.out";

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
                var coordPairs = input.Split(' ').ToArray();

                var visitedPositions = new HashSet<Vector2>();
                var intersectsWithItself = false;

                var startCoords = coordPairs.First().Split(',').Select(int.Parse).ToArray();
                var startPos = new Vector2(startCoords[0], startCoords[1]);
                visitedPositions.Add(startPos);

                var lastPos = startPos;

                foreach (var coordPair in coordPairs.Skip(1))
                {
                    var coords = coordPair.Split(',').Select(int.Parse).ToArray();
                    var pos = new Vector2(coords[0], coords[1]);

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

    private static void Level2()
    {
        for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
        {
            var inputfilename = $"../../../level2_{inputFileNumber}.in";
            var outputfilename = $"../../../level2_{inputFileNumber}.out";

            Console.WriteLine(inputfilename);

            var lines = File.ReadAllLines(inputfilename).ToList();

            using var outputWriter = new StreamWriter(outputfilename);

            int mapHeight = int.Parse(lines.First());

            var map = lines.Skip(1).Take(mapHeight).ToList();

            var inputs = lines.Skip(1 + mapHeight + 1).ToList();
            var output = new StringBuilder();

            foreach (var input in inputs)
            {
                var coordPair = input.Split(' ').ToArray();
                var coords1 = coordPair[0].Split(',').Select(int.Parse).ToArray();
                var pos1 = new Vector2(coords1[0], coords1[1]);

                var coords2 = coordPair[1].Split(',').Select(int.Parse).ToArray();
                var pos2 = new Vector2(coords2[0], coords2[1]);

                var availablePositions = new Stack<Vector2>();
                availablePositions.Push(pos1);

                var visitedPositions = new HashSet<Vector2>();

                bool isOnSameIsland = false;

                var directions = new List<Vector2>()
                {
                    {new Vector2(1, 0) },
                    {new Vector2(-1, 0) },
                    {new Vector2(0, 1) },
                    {new Vector2(0, -1) }
                };

                var mapWidth = map.First().Length;

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

                        if (candidate.Y < 0 || candidate.Y >= map.Count ||
                            candidate.X < 0 || candidate.X >= mapWidth)
                        {
                            continue; // out of bounds
                        }

                        if (!visitedPositions.Contains(candidate) && map[(int)candidate.Y][(int)candidate.X] == 'L')
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

    private static void Level1()
    {
        for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
        {
            var inputfilename = $"../../../level1_{inputFileNumber}.in";
            var outputfilename = $"../../../level1_{inputFileNumber}.out";

            Console.WriteLine(inputfilename);

            var lines = File.ReadAllLines(inputfilename).ToList();

            using var outputWriter = new StreamWriter(outputfilename);

            int mapHeight = int.Parse(lines.First());

            var map = lines.Skip(1).Take(mapHeight).ToList();

            var inputs = lines.Skip(1 + mapHeight + 1).ToList();
            var output = new StringBuilder();

            foreach (var input in inputs)
            {
                var coords = input.Split(',').Select(int.Parse).ToArray();
                var x = coords[0];
                var y = coords[1];

                output.Append(map[y][x]);
                output.AppendLine();
            }

            outputWriter.Write(output.ToString());
        }
    }
}
