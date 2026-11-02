using PassMaster.Contracts;

namespace PassMaster.Runner.Harness;

/// <summary>
/// История студента. Хранится в списке, потому что количество дней
/// в live-режиме не ограничено.
/// </summary>
public sealed class StudentHistory : IStudentHistory
{
    private sealed class DayEntry
    {
        public readonly bool[] Attended = new bool[Constants.SubjectsCount];
        public readonly bool[] Asked = new bool[Constants.SubjectsCount];
        public readonly bool[] AskedKnown = new bool[Constants.SubjectsCount];
    }

    private readonly List<DayEntry> _days = new();

    public void Record(int day, Subject subject, bool attended, bool asked)
    {
        if (day < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(day), "Номер дня не может быть отрицательным.");
        }

        EnsureDayExists(day);

        int s = (int)subject;
        DayEntry entry = _days[day];

        entry.Attended[s] = attended;
        entry.Asked[s] = asked;

        // Вопрос известен, только если студент был на паре.
        entry.AskedKnown[s] = attended;
    }

    public bool Attended(int day, Subject subject)
    {
        if (day < 0 || day >= _days.Count)
        {
            return false;
        }

        return _days[day].Attended[(int)subject];
    }

    public bool? WasAsked(int day, Subject subject)
    {
        if (day < 0 || day >= _days.Count)
        {
            return null;
        }

        DayEntry entry = _days[day];
        int s = (int)subject;

        return entry.AskedKnown[s] ? entry.Asked[s] : null;
    }

    private void EnsureDayExists(int day)
    {
        while (_days.Count <= day)
        {
            _days.Add(new DayEntry());
        }
    }
}
