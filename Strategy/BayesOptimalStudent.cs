using PassMaster.Contracts;

namespace PassMaster.Strategy;

/// <summary>
/// Байесовская стратегия: по истории оценивает правила преподавателей
/// и выбирает прогулы, максимизирующие ожидаемое удовольствие.
/// Для каждого предмета хранится 37 гипотез: Random, 6 вариантов AskIfA
/// и 30 вариантов AskIfXorAB.
/// </summary>
public sealed class BayesOptimalStudent : ISkipStrategy
{
    private const int Subjects = Constants.SubjectsCount;

    // 1 (Random) + 6 (AskIfA) + 6*5 (AskIfXorAB).
    private const int HypothesisCount = 1 + Subjects + Subjects * (Subjects - 1);

    private const int RandomHypothesis = 0;
    private const int FirstAskIfA = 1;
    private const int FirstXor = 1 + Subjects;

    // Риск ограничен: прогуливаем, только если вероятность вопроса не больше этого порога.
    private const double MaxAskProbability = 0.005;

    private readonly double[,] _weights = new double[Subjects, HypothesisCount];
    private readonly int[] _hypothesisA = new int[HypothesisCount];
    private readonly int[] _hypothesisB = new int[HypothesisCount];

    // Кого спрашивали вчера. В первый день все false.
    private readonly bool[] _qPrev = new bool[Subjects];
    private readonly bool[] _nextQ = new bool[Subjects];

    // Фактическое удовольствие стратегии неизвестно, поэтому храним ожидаемое.
    private double _expectedPleasure;

    public string Name => "BayesOptimal";

    public BayesOptimalStudent()
    {
        InitializeHypotheses();
        Reset();
    }

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        // День 0 — начало нового прогона, состояние нужно сбросить.
        if (day == 0)
        {
            Reset();
        }
        else
        {
            UpdateFromYesterday(day, history);
        }

        return MakeDecisions();
    }

    private void Reset()
    {
        _expectedPleasure = 0.0;

        for (int s = 0; s < Subjects; s++)
        {
            _qPrev[s] = false;

            for (int h = 0; h < HypothesisCount; h++)
            {
                _weights[s, h] = PriorWeight(h);
            }
        }
    }

    private void InitializeHypotheses()
    {
        _hypothesisA[RandomHypothesis] = -1;
        _hypothesisB[RandomHypothesis] = -1;

        int index = FirstAskIfA;
        for (int a = 0; a < Subjects; a++)
        {
            _hypothesisA[index] = a;
            _hypothesisB[index] = -1;
            index++;
        }

        for (int a = 0; a < Subjects; a++)
        {
            for (int b = 0; b < Subjects; b++)
            {
                if (b == a)
                {
                    continue;
                }

                _hypothesisA[index] = a;
                _hypothesisB[index] = b;
                index++;
            }
        }
    }

    private static double PriorWeight(int hypothesis)
    {
        if (hypothesis == RandomHypothesis)
        {
            return 1.0 / 3.0;
        }

        if (hypothesis < FirstXor)
        {
            return 1.0 / 18.0;
        }

        return 1.0 / 90.0;
    }

    private void UpdateFromYesterday(int day, IReadOnlyStudentHistory history)
    {
        int observedDay = day - 1;

        // Пересчитываем веса по вчерашним ответам. _qPrev ещё старый — так и надо.
        for (int s = 0; s < Subjects; s++)
        {
            if (!history.Attended(observedDay, (Subject)s))
            {
                continue;
            }

            bool? asked = history.WasAsked(observedDay, (Subject)s);
            if (asked.HasValue)
            {
                UpdateWeights(s, asked.Value);
            }
        }

        // Строим вектор q на сегодня.
        for (int s = 0; s < Subjects; s++)
        {
            bool attended = history.Attended(observedDay, (Subject)s);
            bool? asked = attended ? history.WasAsked(observedDay, (Subject)s) : null;

            // Если прогуляли, считаем, что не спросили: прогуливаем только при малой вероятности.
            _nextQ[s] = asked ?? false;
        }

        Array.Copy(_nextQ, _qPrev, Subjects);
    }

    private void UpdateWeights(int subject, bool asked)
    {
        double sum = 0.0;

        for (int h = 0; h < HypothesisCount; h++)
        {
            double probability = AskProbability(subject, h);
            double likelihood = asked ? probability : 1.0 - probability;

            _weights[subject, h] *= likelihood;
            sum += _weights[subject, h];
        }

        // Страховка от деления на ноль.
        if (sum <= 0.0 || double.IsNaN(sum))
        {
            for (int h = 0; h < HypothesisCount; h++)
            {
                _weights[subject, h] = PriorWeight(h);
            }

            return;
        }

        for (int h = 0; h < HypothesisCount; h++)
        {
            _weights[subject, h] /= sum;
        }
    }

    private double AskProbability(int subject, int hypothesis)
    {
        if (hypothesis == RandomHypothesis)
        {
            return 0.5;
        }

        int a = _hypothesisA[hypothesis];

        if (hypothesis < FirstXor)
        {
            return _qPrev[a] ? 1.0 : 0.0;
        }

        int b = _hypothesisB[hypothesis];
        return (_qPrev[a] ^ _qPrev[b]) ? 1.0 : 0.0;
    }

    private double PosteriorAskProbability(int subject)
    {
        double result = 0.0;

        for (int h = 0; h < HypothesisCount; h++)
        {
            result += _weights[subject, h] * AskProbability(subject, h);
        }

        return result;
    }

    private bool[] MakeDecisions()
    {
        var decisions = new bool[Subjects];
        var pAsk = new double[Subjects];

        for (int s = 0; s < Subjects; s++)
        {
            pAsk[s] = PosteriorAskProbability(s);
        }

        // Перебираем 64 набора прогулов и берём лучший по ожидаемому удовольствию.
        int bestMask = 0;
        double bestScore = double.NegativeInfinity;

        for (int mask = 0; mask < (1 << Subjects); mask++)
        {
            double noCatch = 1.0;
            int skips = 0;
            bool forbidden = false;

            for (int s = 0; s < Subjects; s++)
            {
                if ((mask >> s & 1) == 0)
                {
                    continue;
                }

                if (pAsk[s] > MaxAskProbability)
                {
                    forbidden = true;
                    break;
                }

                noCatch *= 1.0 - pAsk[s];
                skips++;
            }

            if (forbidden)
            {
                continue;
            }

            // Если поймают — удовольствие обнулится, поэтому учитываем только вариант без вылета.
            double score = noCatch * (_expectedPleasure + skips + 1.0);

            if (score > bestScore + 1e-9)
            {
                bestScore = score;
                bestMask = mask;
            }
        }

        _expectedPleasure = bestScore;

        for (int s = 0; s < Subjects; s++)
        {
            decisions[s] = (bestMask >> s & 1) == 0;
        }

        return decisions;
    }
}
