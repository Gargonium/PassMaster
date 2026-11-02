using PassMaster.Contracts;
using PassMaster.Strategy;

namespace PassMaster.Runner.Services;

/// <summary>
/// Создаёт стратегию по имени. Новую стратегию достаточно добавить
/// в switch и в список <see cref="Names"/>.
/// </summary>
public static class StrategyFactory
{
    public static IReadOnlyList<string> Names { get; } =
    [
        "BayesOptimal",
        "BayesSafe",
        "CautiousScout",
        "PrudentStudent",
        "ImprudentStudent",
        "RandomSkip"
    ];

    public static ISkipStrategy Create(string name)
    {
        string key = name.Trim().ToLowerInvariant();

        return key switch
        {
            "bayesoptimal" or "bayesoptimalstudent" or "bayes" or "optimal"
                => new BayesOptimalStudent(),

            "bayessafe" or "bayessafestudent" or "safe"
                => new BayesSafeStudent(),

            "cautiousscout" or "cautious" => new CautiousScout(),
            "prudentstudent" or "prudent" or "alwaysattend" => new PrudentStudent(),
            "imprudentstudent" or "imprudent" or "neverattend" or "skipall" => new ImprudentStudent(),
            "randomskip" or "random" => new RandomSkip(),
            _ => throw new ArgumentException(
                $"Неизвестная стратегия '{name}'. Доступные: {string.Join(", ", Names)}")
        };
    }
}
