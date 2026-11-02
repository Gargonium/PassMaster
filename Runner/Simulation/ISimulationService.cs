using PassMaster.Contracts;

namespace PassMaster.Runner.Simulation;

/// <summary>
/// Результат одного дня. DayNumber считается с нуля.
/// </summary>
public sealed record DayResult(
    int DayNumber,
    int Pleasure,
    int Expulsions,
    bool WasCaught,
    bool Stopped,
    int SuccessfulSkips,
    int AskedCount,
    bool[] Decisions,
    bool[] Asked)
{
    public double AveragePleasure => Pleasure / (double)(DayNumber + 1);
}

/// <summary>
/// Один прогон симуляции: хранит состояние между днями.
/// </summary>
public interface ISimulationService
{
    int Day { get; }

    int Pleasure { get; }

    int Expulsions { get; }

    bool IsStopped { get; }

    IReadOnlyStudentHistory History { get; }

    DayResult AdvanceDay();
}
