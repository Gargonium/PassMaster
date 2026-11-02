using Microsoft.Extensions.Hosting;
using PassMaster.Runner.Configuration;
using PassMaster.Runner.Output;
using PassMaster.Runner.Simulation;

namespace PassMaster.Runner.Hosting;

/// <summary>
/// Игровой цикл: первый день сразу, следующие — через заданный интервал.
/// При вылете хост останавливается.
/// </summary>
public sealed class SimulationWorker : BackgroundService
{
    private readonly ISimulationService _simulation;
    private readonly IOutput _output;
    private readonly SimulationOptions _options;
    private readonly IHostApplicationLifetime _lifetime;

    public SimulationWorker(
        ISimulationService simulation,
        IOutput output,
        SimulationOptions options,
        IHostApplicationLifetime lifetime)
    {
        _simulation = simulation;
        _output = output;
        _options = options;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.DayIntervalSeconds));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DayResult result = _simulation.AdvanceDay();
                _output.PrintDay(result);

                if (result.Stopped)
                {
                    _output.PrintExpelled(result);
                    _lifetime.StopApplication();
                    return;
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Нормальная остановка.
        }
    }
}
