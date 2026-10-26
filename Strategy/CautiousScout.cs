using PassMaster.Contracts;

namespace PassMaster.Strategy;

public class CautiousScout : ISkipStrategy
{
    public string Name => "CautiousScout";

    private const int LearningPhaseDays = 20;

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        var decisions = new bool[Constants.SubjectsCount];
        for (int s = 0; s < Constants.SubjectsCount; s++)
            decisions[s] = ShouldAttend(day, history, (Subject)s);
        return decisions;
    }

    private static bool ShouldAttend(int day, IReadOnlyStudentHistory history, Subject subject)
    {
        if (day < LearningPhaseDays) return true;

        for (int d = 0; d < LearningPhaseDays; d++)
            if (history.WasAsked(d, subject) == true)
                return true;

        return false;
    }
}