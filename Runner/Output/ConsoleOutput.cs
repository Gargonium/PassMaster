using PassMaster.Contracts;
using PassMaster.Runner.Harness;
using PassMaster.Runner.Simulation;

namespace PassMaster.Runner.Output;

public sealed class ConsoleOutput : IOutput
{
    // Вывод идёт из двух потоков, блокировка не даёт строкам перемешаться.
    private readonly object _sync = new();

    public void PrintStart(string strategyName, int intervalSeconds)
    {
        lock (_sync)
        {
            Console.WriteLine();
            Console.WriteLine($"=== PassMaster: {strategyName} ===");
            Console.WriteLine($"Новый день каждые {intervalSeconds} сек. Нажмите любую клавишу для остановки.");
            Console.WriteLine();
        }
    }

    public void PrintDay(DayResult result)
    {
        lock (_sync)
        {
            Console.WriteLine($"=== День {result.DayNumber + 1} ===");

            if (result.WasCaught)
            {
                Console.WriteLine("Студент прогулял и его спросили!");
            }

            Console.WriteLine($"Успешных прогулов: {result.SuccessfulSkips}");
            Console.WriteLine($"Спросили:         {result.AskedCount}");
            Console.WriteLine($"Удовольствие:     {result.Pleasure}");
            Console.WriteLine($"Среднее в день:   {result.AveragePleasure:F2}");
            Console.WriteLine();
        }
    }

    public void PrintExpelled(DayResult result)
    {
        lock (_sync)
        {
            Console.WriteLine($"!!! Студент вылетел на дне {result.DayNumber + 1}. Удовольствие обнулено. Остановка. !!!");
            Console.WriteLine();
        }
    }

    public void PrintStoppedByKey()
    {
        lock (_sync)
        {
            Console.WriteLine();
            Console.WriteLine("Остановка по нажатию клавиши.");
        }
    }

    public void PrintBatchSummary(RunSummary summary, string strategyName)
    {
        lock (_sync)
        {
            Console.WriteLine($"=== {strategyName} ===");
            Console.WriteLine($"Среднее удовольствие за итерации:           {summary.AveragePleasure:F3}");
            Console.WriteLine($"Среднее удовольствие за итерации за день:   {summary.AveragePleasure / Constants.DaysCount:F3}");
            Console.WriteLine($"Среднее число вылетов:                      {summary.AverageExpulsions:F3}");
            Console.WriteLine($"Максимум удовольствия:                      {summary.MaxFun:F3}");
        }
    }
}
