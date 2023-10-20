namespace CCC;

public class Program
{
    public static void Main(string[] args)
    {
        Level1();
        //Level2();
        //Level3();
        //Level4();
        //Level5();

        Console.WriteLine("Done");
    }

    private static void Level1()
    {
        for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
        {
            var inputfilename = $"../../../level1_{inputFileNumber}.in";
            var outputfilename = $"../../../level1_{inputFileNumber}.out";

            Console.WriteLine(inputfilename);

            var lines = File.ReadAllLines(inputfilename).ToList();
            var tournaments = lines.Skip(1).ToList();

            using var outputWriter = new StreamWriter(outputfilename);


            foreach (var input in tournaments)
            {


            }
        }
    }
}
