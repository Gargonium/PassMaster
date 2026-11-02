using System.Globalization;
using PassMaster.Contracts;

namespace PassMaster.Runner.Configuration;

/// <summary>
/// Разбор аргументов командной строки.
///
/// Примеры:
///   dotnet run
///   dotnet run -- --strategy PrudentStudent --interval 2
///   dotnet run -- --batch --strategy CautiousScout
/// </summary>
public sealed class CommandLineOptions
{
    public string StrategyName { get; private init; } = "BayesOptimal";

    public int DayIntervalSeconds { get; private init; } = Constants.DayIntervalSeconds;

    public bool BatchMode { get; private init; }

    // -1 означает случайный seed.
    public int Seed { get; private init; } = -1;

    public bool ShowHelp { get; private init; }

    public static CommandLineOptions Parse(string[] args)
    {
        string strategy = "BayesOptimal";
        int interval = Constants.DayIntervalSeconds;
        int seed = -1;
        bool batch = false;
        bool help = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            switch (arg)
            {
                case "--batch":
                    batch = true;
                    break;

                case "--help":
                case "-h":
                    help = true;
                    break;

                case "--strategy":
                    strategy = ReadValue(args, ref i, arg);
                    break;

                case "--interval":
                    interval = ParseInterval(ReadValue(args, ref i, arg));
                    break;

                case "--seed":
                    seed = ParseSeed(ReadValue(args, ref i, arg));
                    break;

                default:
                    if (arg.StartsWith("--strategy=", StringComparison.OrdinalIgnoreCase))
                    {
                        strategy = arg["--strategy=".Length..];
                    }
                    else if (arg.StartsWith("--interval=", StringComparison.OrdinalIgnoreCase))
                    {
                        interval = ParseInterval(arg["--interval=".Length..]);
                    }
                    else if (arg.StartsWith("--seed=", StringComparison.OrdinalIgnoreCase))
                    {
                        seed = ParseSeed(arg["--seed=".Length..]);
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"Неизвестный аргумент '{arg}'. Используйте --help, чтобы увидеть список параметров.");
                    }

                    break;
            }
        }

        return new CommandLineOptions
        {
            StrategyName = strategy,
            DayIntervalSeconds = interval,
            Seed = seed,
            BatchMode = batch,
            ShowHelp = help
        };
    }

    private static string ReadValue(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Для параметра {option} не указано значение.");
        }

        return args[++index];
    }

    private static int ParseInterval(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int interval) || interval <= 0)
        {
            throw new ArgumentException($"Интервал должен быть положительным целым числом, получено '{value}'.");
        }

        return interval;
    }

    private static int ParseSeed(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int seed))
        {
            throw new ArgumentException($"Seed должен быть целым числом, получено '{value}'.");
        }

        return seed;
    }
}
