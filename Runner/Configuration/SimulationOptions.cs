using PassMaster.Contracts;

namespace PassMaster.Runner.Configuration;

/// <summary>
/// Настройки симуляции: интервал между днями и поведение при вылете.
/// </summary>
public sealed class SimulationOptions
{
    public int DayIntervalSeconds { get; init; } = Constants.DayIntervalSeconds;

    // true — live-режим: остановиться при вылете.
    // false — batch: вылет обнуляет удовольствие, но прогон продолжается.
    public bool StopOnExpulsion { get; init; } = true;
}
