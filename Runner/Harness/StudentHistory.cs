using PassMaster.Contracts;

namespace PassMaster.Runner.Harness;

public class StudentHistory : IReadOnlyStudentHistory
{
    private readonly bool[,] _attended = new bool[Constants.DaysCount, Constants.SubjectsCount];
    private readonly bool[,] _asked = new bool[Constants.DaysCount, Constants.SubjectsCount];
    private readonly bool[,] _askedKnown = new bool[Constants.DaysCount, Constants.SubjectsCount];

    public void Record(int day, int subject, bool attended, bool asked)
    {
        _attended[day, subject] = attended;
        _asked[day, subject] = asked;
        _askedKnown[day, subject] = attended;
    }
    
    public bool Attended(int day, Subject subject)
    {
        return _attended[day, (int)subject];
    }

    public bool? WasAsked(int day, Subject subject)
    {
        if (!_askedKnown[day, (int)subject]) return null;
        return _asked[day, (int)subject];
    }
}