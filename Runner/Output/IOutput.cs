using PassMaster.Runner.Harness;
using PassMaster.Runner.Simulation;

namespace PassMaster.Runner.Output;

public interface IOutput
{
    void PrintStart(string strategyName, int intervalSeconds);

    void PrintDay(DayResult result);

    void PrintExpelled(DayResult result);

    void PrintStoppedByKey();

    void PrintBatchSummary(RunSummary summary, string strategyName);
}
