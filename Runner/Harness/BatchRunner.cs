using PassMaster.Contracts;

namespace PassMaster.Runner.Harness;

public record RunSummary(double AveragePleasure, double AverageExpulsions);

public static class BatchRunner
{
    public static RunSummary RunMany(Func<ISkipStrategy> factory, int iterations, int seed)
    {
        Random rnd = new Random(seed);
        int totalPleasure = 0;
        int totalExpulsions = 0;

        for (int i = 0; i < iterations; i++)
        {
            var result = Simulation.Simulation.Run(factory(), rnd);
            totalPleasure += result.Pleasure;
            totalExpulsions += result.Expulsions;
        }
        return new RunSummary((double)totalPleasure / iterations, (double)totalExpulsions / iterations);
    }
}