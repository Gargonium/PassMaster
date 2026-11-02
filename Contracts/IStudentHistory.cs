namespace PassMaster.Contracts;

/// <summary>
/// История студента: симуляция записывает сюда результаты каждого дня.
/// Стратегия получает только <see cref="IReadOnlyStudentHistory"/>.
/// </summary>
public interface IStudentHistory : IReadOnlyStudentHistory
{
    void Record(int day, Subject subject, bool attended, bool asked);
}
