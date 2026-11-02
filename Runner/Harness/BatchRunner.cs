using PassMaster.Contracts;
using PassMaster.Runner.Configuration;
using PassMaster.Runner.Services;
using PassMaster.Runner.Simulation;

namespace PassMaster.Runner.Harness;

public record RunSummary(double AveragePleasure, double AverageExpulsions, int MaxFun);

/// <summary>
/// Batch-режим: много семестров подряд и средняя статистика.
/// Использует тот же SimulationService, но с StopOnExpulsion = false.
/// </summary>
public static class BatchRunner
{
    public static RunSummary RunMany(Func<ISkipStrategy> factory, int iterations, int seed)
    {
        var random = new RandomSource(seed);
        var teacherFactory = new TeacherFactory(random);

        int totalPleasure = 0;
        int totalExpulsions = 0;
        int maxFun = 0;

        for (int i = 0; i < iterations; i++)
        {
            var history = new StudentHistory();
            var options = new SimulationOptions { StopOnExpulsion = false };
            var simulation = new SimulationService(factory(), teacherFactory, history, options);

            for (int day = 0; day < Constants.DaysCount; day++)
            {
                simulation.AdvanceDay();
            }

            totalPleasure += simulation.Pleasure;
            totalExpulsions += simulation.Expulsions;

            if (simulation.Pleasure > maxFun)
            {
                maxFun = simulation.Pleasure;
            }
        }

        return new RunSummary(
            (double)totalPleasure / iterations,
            (double)totalExpulsions / iterations,
            maxFun);
    }
}
