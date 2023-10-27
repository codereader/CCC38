using IslandsLib;

namespace CCC;

public class Program
{
    public static void Main(string[] args)
    {
        var navigator = new Navigator();

        for (int i = 0; i < 8; i++)
        {
            navigator.WriteOutputFiles(i);
        }

        Console.WriteLine("Done");
    }
}
