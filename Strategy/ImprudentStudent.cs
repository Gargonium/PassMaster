using PassMaster.Contracts;

namespace PassMaster.Strategy;

public class ImprudentStudent : ISkipStrategy
{
    public string Name => "ImprudentStudent (never attend)";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
        => [false, false, false, false, false, false];
}