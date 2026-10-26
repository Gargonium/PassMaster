using PassMaster.Contracts;

namespace PassMaster.Strategy;

public class RandomSkip : ISkipStrategy
{
    public string Name => "RandomStudent (random attend)";

    private readonly Random _rnd = new Random();

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        bool [] attendance = new bool[Constants.SubjectsCount];
        for (int i = 0; i < Constants.SubjectsCount; i++)
        {
            attendance[i] = _rnd.Next(2) == 0;
        }

        return attendance;
    }
}