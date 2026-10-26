using PassMaster.Contracts;
using PassMaster.Runner.Harness;
using PassMaster.Strategy;

namespace PassMaster.Runner;

public static class Program
{
    public static void Main()
    {
        const int iterations = 1000;
        
        ISkipStrategy chosenStrategy = new CautiousScout();
        
        var summary = BatchRunner.RunMany(() => chosenStrategy, iterations, seed: DateTime.UtcNow.Second);
        Console.WriteLine($"=== {chosenStrategy.Name} ===");
        Console.WriteLine($"Среднее удовольствие:      {summary.AveragePleasure:F3}");
        Console.WriteLine($"Среднее число вылетов:     {summary.AverageExpulsions:F3}");
        
    }
}

