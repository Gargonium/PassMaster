using PassMaster.Contracts;

namespace PassMaster.Strategy;

public class PrudentStudent : ISkipStrategy
{
    public string Name => "PrudentStudent (always attend)";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
        => [true, true, true, true, true, true];
}