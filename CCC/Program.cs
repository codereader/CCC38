using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace CCC;

public class Program
{
    public static void Main(string[] args)
    {
        //Level1();
        //Level2();
        //Level3();
        Level4();
        //Level5();

        Console.WriteLine("Done");
    }

    public class Route
    {
        private HashSet<Vector2> _halfPoints = new();
        private List<Vector2> _points = new();

        public Vector2 EndPoint => _points[_points.Count - 1];

        public int Length => _points.Count;

        internal void AddStartPoint(Vector2 point)
        {
            _points.Add(point);
            _halfPoints.Add(point);
        }

        internal void AddPoint(Vector2 targetPoint)
        {
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

        public class LengthComparer : IComparer<Route>
        {
            public int Compare(Route? x, Route? y)
            {
                return x.Length.CompareTo(y.Length);
            }
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

                var directions = new List<Vector2>()
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
        for (var inputFileNumber = 3; inputFileNumber <= 5; inputFileNumber++)
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

                        if(!visitedPositions.Contains(candidate) && map[(int)candidate.Y][(int)candidate.X] == 'L')
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
