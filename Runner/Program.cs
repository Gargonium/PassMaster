using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PassMaster.Contracts;
using PassMaster.Runner.Configuration;
using PassMaster.Runner.Harness;
using PassMaster.Runner.Hosting;
using PassMaster.Runner.Output;
using PassMaster.Runner.Services;
using PassMaster.Runner.Simulation;

namespace PassMaster.Runner;

public static class Program
{
    public static void Main(string[] args)
    {
        // Чтобы русский текст нормально выводился и в консоли, и в файл.
        Console.OutputEncoding = Encoding.UTF8;

        CommandLineOptions cli;

        try
        {
            cli = CommandLineOptions.Parse(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            PrintHelp();
            Environment.ExitCode = 1;
            return;
        }

        if (cli.ShowHelp)
        {
            PrintHelp();
            return;
        }

        ISkipStrategy strategy;
        try
        {
            strategy = StrategyFactory.Create(cli.StrategyName);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            PrintHelp();
            Environment.ExitCode = 1;
            return;
        }

        if (cli.BatchMode)
        {
            RunBatch(cli, strategy);
            return;
        }

        RunLive(cli, strategy);
    }

    /// <summary>
    /// Live-режим на Generic Host: дни идут, пока не вылетит или не нажмут клавишу.
    /// </summary>
    private static void RunLive(CommandLineOptions cli, ISkipStrategy strategy)
    {
        var builder = Host.CreateApplicationBuilder();

        // Логи хоста не нужны, они засоряют вывод.
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.Services.AddSingleton(new SimulationOptions
        {
            DayIntervalSeconds = cli.DayIntervalSeconds,
            StopOnExpulsion = true
        });

        builder.Services.AddSingleton<IRandomSource, RandomSource>();
        builder.Services.AddSingleton<ITeacherFactory, TeacherFactory>();
        builder.Services.AddSingleton<IStudentHistory, StudentHistory>();
        builder.Services.AddSingleton(strategy);
        builder.Services.AddSingleton<ISimulationService, SimulationService>();
        builder.Services.AddSingleton<IOutput, ConsoleOutput>();

        builder.Services.AddHostedService<SimulationWorker>();
        builder.Services.AddHostedService<KeyPressWatcher>();

        using IHost host = builder.Build();

        // Заголовок печатаем до запуска сервисов, чтобы он был первым.
        IOutput output = host.Services.GetRequiredService<IOutput>();
        output.PrintStart(strategy.Name, cli.DayIntervalSeconds);

        host.Run();
    }

    /// <summary>
    /// Batch-режим: много семестров и средняя статистика.
    /// </summary>
    private static void RunBatch(CommandLineOptions cli, ISkipStrategy strategy)
    {
        int seed = cli.Seed >= 0 ? cli.Seed : DateTime.UtcNow.Second;

        RunSummary summary = BatchRunner.RunMany(
            () => strategy,
            Constants.Iterations,
            seed);

        new ConsoleOutput().PrintBatchSummary(summary, strategy.Name);
    }

    private static void PrintHelp()
    {
        Console.WriteLine($"""
            Использование:
              dotnet run -- [--strategy <имя>] [--interval <секунды>]
              dotnet run -- --batch [--strategy <имя>]

            Параметры:
              --strategy <имя>   Стратегия студента (по умолчанию BayesOptimal).
              --interval <сек>   Интервал между днями в live-режиме (по умолчанию 5).
              --batch            Режим лабораторной №1: 1000 прогонов и статистика.
              --seed <число>     Seed для batch-прогона (для воспроизводимости).
              --help, -h         Показать эту справку.

            Доступные стратегии:
              {string.Join(", ", StrategyFactory.Names)}

            Байесовские стратегии:
              BayesOptimal — максимум ожидаемого удовольствия (~346 за семестр,
                             но примерно раз в семестр бывает вылет).
              BayesSafe    — строго без вылетов (~290 за семестр), для live-режима.

            Примеры:
              dotnet run
              dotnet run -- --strategy PrudentStudent --interval 2
              dotnet run -- --batch --strategy RandomSkip
            """);
    }
}
