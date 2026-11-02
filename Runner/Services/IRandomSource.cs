namespace PassMaster.Runner.Services;

/// <summary>
/// Источник случайности. Отдельный интерфейс нужен, чтобы в тестах
/// подменить Random управляемой последовательностью.
/// </summary>
public interface IRandomSource
{
    int Next(int maxExclusive);

    bool NextBool();
}
