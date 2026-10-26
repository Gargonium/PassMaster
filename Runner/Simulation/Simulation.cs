using PassMaster.Contracts;
using PassMaster.Runner.Harness;

namespace PassMaster.Runner.Simulation;

public record RunResult(int Pleasure, int Expulsions);

public static class Simulation
{
    public static RunResult Run(ISkipStrategy strategy, Random rnd)
    {
        Teacher[] teachers = new Teacher[Constants.SubjectsCount];
        for (int i = 0; i < Constants.SubjectsCount; i++)
        {
            teachers[i] = new Teacher((Subject)i, rnd);
        }
        
        StudentHistory history = new StudentHistory();
        bool[] yesterdayAsked = new bool[Constants.SubjectsCount];

        int pleasure = 0;
        int expulsions = 0;

        for (int day = 0; day < Constants.DaysCount; day++)
        {
            bool[] decisions = strategy.DecideDay(day, history);
            if (decisions.Length != Constants.SubjectsCount)
                throw new InvalidOperationException($"DecideDay must return array of length {Constants.SubjectsCount}");
            
            bool[] todayAsked = new bool[Constants.SubjectsCount];
            for (int s = 0; s < Constants.SubjectsCount; s++)
            {
                bool attend = decisions[s];
                bool asked = teachers[s].ShouldAsk(yesterdayAsked, rnd);
                todayAsked[s] = asked;
                
                history.Record(day, s, attend, asked);

                if (!attend && asked)
                {
                    pleasure = 0;
                    expulsions++;
                }
                else if (!attend)
                {
                    pleasure += Constants.PleasurePerSkip;
                }
            }

            pleasure += Constants.PleasurePerPie;
            yesterdayAsked = todayAsked;
        }
        return new RunResult(pleasure, expulsions);
    }
}