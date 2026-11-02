using PassMaster.Contracts;
using PassMaster.Runner.Services;

namespace PassMaster.Runner.Simulation;

/// <summary>
/// Преподаватель одного предмета. Правило и предметы A/B
/// выбираются случайно один раз при создании.
/// </summary>
public class Teacher
{
    private readonly IRandomSource _random;

    public Subject Subject { get; }

    public TeacherRule Rule { get; }

    public Subject? SubjectA { get; }

    public Subject? SubjectB { get; }

    public Teacher(Subject subject, IRandomSource random)
    {
        Subject = subject;
        _random = random;

        Rule = (TeacherRule)_random.Next(Constants.RulesCount);

        if (Rule == TeacherRule.Random)
        {
            SubjectA = null;
            SubjectB = null;
        }
        else
        {
            // A может совпадать с собственным предметом преподавателя.
            Subject a = (Subject)_random.Next(Constants.SubjectsCount);

            Subject b;
            do
            {
                b = (Subject)_random.Next(Constants.SubjectsCount);
            } while (b == a);

            SubjectA = a;
            SubjectB = b;
        }
    }

    /// <summary>
    /// Спрашивает ли преподаватель сегодня. В первый день yesterdayAsked весь false.
    /// </summary>
    public bool ShouldAsk(bool[] yesterdayAsked)
    {
        switch (Rule)
        {
            case TeacherRule.Random:
                return _random.NextBool();

            case TeacherRule.AskIfA:
                return yesterdayAsked[(int)SubjectA!.Value];

            case TeacherRule.AskIfXorAB:
                bool a = yesterdayAsked[(int)SubjectA!.Value];
                bool b = yesterdayAsked[(int)SubjectB!.Value];
                return a ^ b;

            default:
                throw new InvalidOperationException($"Неизвестное правило преподавателя: {Rule}");
        }
    }
}
