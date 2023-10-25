namespace IslandsLib;

public class Scenario
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
        Level = Level;
    }

    public int Level { get; }

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

