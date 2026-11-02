using Microsoft.Extensions.Hosting;
using PassMaster.Runner.Output;

namespace PassMaster.Runner.Hosting;

/// <summary>
/// Останавливает приложение по нажатию любой клавиши.
/// Console.KeyAvailable не работает при перенаправленном вводе,
/// поэтому для этого случая читаем stdin.
/// </summary>
public sealed class KeyPressWatcher : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IOutput _output;

    public KeyPressWatcher(IHostApplicationLifetime lifetime, IOutput output)
    {
        _lifetime = lifetime;
        _output = output;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Чтение ввода не должно задерживать остановку хоста.
        _ = Task.Run(() => WatchAsync(stoppingToken), CancellationToken.None);
        return Task.CompletedTask;
    }

    private async Task WatchAsync(CancellationToken stoppingToken)
    {
        if (Console.IsInputRedirected)
        {
            await WatchRedirectedInputAsync(stoppingToken);
        }
        else
        {
            await WatchConsoleKeysAsync(stoppingToken);
        }
    }

    private async Task WatchConsoleKeysAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(intercept: true);
                    Stop();
                    return;
                }

                await Task.Delay(100, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Нормальная остановка.
        }
        catch (InvalidOperationException)
        {
            // Ввод недоступен — просто не следим за клавишами.
        }
    }

    private async Task WatchRedirectedInputAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using Stream stdin = Console.OpenStandardInput();
            var buffer = new byte[1];

            while (!stoppingToken.IsCancellationRequested)
            {
                int read = await stdin.ReadAsync(buffer.AsMemory(0, 1), stoppingToken);

                // 0 — конец потока, интерактивного ввода нет.
                if (read == 0)
                {
                    return;
                }

                Stop();
                return;
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Нормальная остановка.
        }
        catch (IOException)
        {
            // Поток ввода недоступен.
        }
        catch (InvalidOperationException)
        {
            // Ввод недоступен.
        }
    }

    private void Stop()
    {
        _output.PrintStoppedByKey();
        _lifetime.StopApplication();
    }
}
