using PassMaster.Contracts;
using PassMaster.Runner.Services;

namespace PassMaster.Runner.Simulation;

/// <summary>
/// Создаёт преподавателей для нового прогона.
/// </summary>
public interface ITeacherFactory
{
    Teacher[] CreateAll();
}

public sealed class TeacherFactory : ITeacherFactory
{
    private readonly IRandomSource _random;

    public TeacherFactory(IRandomSource random)
    {
        _random = random;
    }

    public Teacher[] CreateAll()
    {
        var teachers = new Teacher[Constants.SubjectsCount];

        for (int i = 0; i < Constants.SubjectsCount; i++)
        {
            teachers[i] = new Teacher((Subject)i, _random);
        }

        return teachers;
    }
}
