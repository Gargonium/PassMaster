namespace PassMaster.Contracts;

public interface ISkipStrategy
{
    string Name { get; }
    bool[] DecideDay(int day, IReadOnlyStudentHistory history);
}

