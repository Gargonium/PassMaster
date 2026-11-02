namespace PassMaster.Contracts;

public static class Constants
{
    public const int Iterations = 1000;

    // Длина семестра в batch-режиме. В live-режиме дни не ограничены.
    public const int DaysCount = 100;

    public const int RulesCount = 3;

    public const int SubjectsCount = 6;

    // Интервал между днями в live-режиме по умолчанию.
    public const int DayIntervalSeconds = 5;
}
