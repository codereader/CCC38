using IslandsLib;

namespace CCC;

public class Program
{
    public static void Main(string[] args)
    {
        var navigator = new Navigator();

        navigator.Level1();
        navigator.Level2();
        navigator.Level3();
        navigator.Level4();
        navigator.Level5_6(5);
        navigator.Level5_6(6);
        navigator.Level7();

        Console.WriteLine("Done");
    }
}
