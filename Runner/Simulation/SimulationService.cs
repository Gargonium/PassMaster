using PassMaster.Contracts;
using PassMaster.Runner.Configuration;

namespace PassMaster.Runner.Simulation;

/// <summary>
/// Логика одного прогона: состояние между днями и правила дня.
/// Количество дней не ограничено.
/// </summary>
public sealed class SimulationService : ISimulationService
{
    private readonly ISkipStrategy _strategy;
    private readonly IStudentHistory _history;
    private readonly SimulationOptions _options;
    private readonly Teacher[] _teachers;

    // Кого спрашивали вчера, по каждому предмету.
    private readonly bool[] _yesterdayAsked = new bool[Constants.SubjectsCount];

    private int _day;
    private int _pleasure;
    private int _expulsions;
    private bool _isStopped;

    public SimulationService(
        ISkipStrategy strategy,
        ITeacherFactory teacherFactory,
        IStudentHistory history,
        SimulationOptions options)
    {
        _strategy = strategy;
        _history = history;
        _options = options;

        // Правила преподавателей выбираются один раз на весь прогон.
        _teachers = teacherFactory.CreateAll();
    }

    public int Day => _day;

    public int Pleasure => _pleasure;

    public int Expulsions => _expulsions;

    public bool IsStopped => _isStopped;

    public IReadOnlyStudentHistory History => _history;

    public DayResult AdvanceDay()
    {
        if (_isStopped)
        {
            throw new InvalidOperationException("Симуляция уже остановлена: дальнейшие дни не обрабатываются.");
        }

        int playedDay = _day;

        // Решение принимается до того, как известны сегодняшние ответы.
        bool[] decisions = _strategy.DecideDay(playedDay, _history);
        if (decisions is null || decisions.Length != Constants.SubjectsCount)
        {
            throw new InvalidOperationException(
                $"DecideDay должен вернуть массив длины {Constants.SubjectsCount}.");
        }

        var todayAsked = new bool[Constants.SubjectsCount];

        bool caughtToday = false;
        bool stopped = false;
        int successfulSkips = 0;
        int askedCount = 0;

        for (int s = 0; s < Constants.SubjectsCount; s++)
        {
            bool attend = decisions[s];

            bool asked = _teachers[s].ShouldAsk(_yesterdayAsked);
            todayAsked[s] = asked;

            _history.Record(playedDay, (Subject)s, attend, asked);

            if (asked)
            {
                askedCount++;
            }

            if (!attend && asked)
            {
                // Вылет: накопленное удовольствие обнуляется.
                _pleasure = 0;
                _expulsions++;
                caughtToday = true;

                if (_options.StopOnExpulsion)
                {
                    stopped = true;
                    _isStopped = true;
                    break;
                }

                continue;
            }

            if (!attend && !caughtToday)
            {
                _pleasure++;
                successfulSkips++;
            }
        }

        if (!caughtToday)
        {
            // Пирожок в конце дня. В день вылета не начисляется.
            _pleasure++;
        }

        Array.Copy(todayAsked, _yesterdayAsked, todayAsked.Length);

        if (!stopped)
        {
            _day++;
        }

        return new DayResult(
            DayNumber: playedDay,
            Pleasure: _pleasure,
            Expulsions: _expulsions,
            WasCaught: caughtToday,
            Stopped: stopped,
            SuccessfulSkips: successfulSkips,
            AskedCount: askedCount,
            Decisions: decisions,
            Asked: todayAsked);
    }
}
