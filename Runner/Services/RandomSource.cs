namespace PassMaster.Runner.Services;

/// <summary>
/// Обычный Random, но экземпляр можно создать с seed и повторить последовательность.
/// </summary>
public sealed class RandomSource : IRandomSource
{
    private readonly Random _random;

    public RandomSource()
        : this(Random.Shared.Next())
    {
    }

    public RandomSource(int seed)
    {
        _random = new Random(seed);
    }

    public int Next(int maxExclusive) => _random.Next(maxExclusive);

    public bool NextBool() => _random.Next(2) == 0;
}
