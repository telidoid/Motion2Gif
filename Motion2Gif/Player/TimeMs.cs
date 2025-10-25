using System;
using System.Numerics;

namespace Motion2Gif.Player;

public readonly record struct TimeMs(long Value)
    : IComparable<TimeMs>, IComparisonOperators<TimeMs, TimeMs, bool>
{
    public static TimeMs FromDip(double dip, double width, TimeMs duration)
    {
        var totalMs = Math.Max(0L, duration.Value); // защита от нулей/отрицательных

        if (width <= 0 || totalMs == 0)
            return new TimeMs(0);

        var ratio = dip / width; // доля пройденного пути по ширине [0..1]
        var ms = Math.Clamp(ratio, 0.0, 1.0) * totalMs; // кламп доли и перевод в миллисекунды

        return new TimeMs((long)Math.Round(ms));
    }

    public int CompareTo(TimeMs other) => Value.CompareTo(other.Value);
    public static bool operator <(TimeMs left, TimeMs right) => left.Value < right.Value;
    public static bool operator >(TimeMs left, TimeMs right) => left.Value > right.Value;
    public static bool operator <=(TimeMs left, TimeMs right) => left.Value <= right.Value;
    public static bool operator >=(TimeMs left, TimeMs right) => left.Value >= right.Value;
}

public static class TimeMsExtensions
{
    private static TimeSpan TimeStamp(this TimeMs time) => TimeSpan.FromMilliseconds(time.Value);

    public static string Formatted(this TimeMs timeMs)
    {
        var ts = timeMs.TimeStamp();

        return ts.Hours == 0
            ? $"{ts.Minutes:00}:{ts.Seconds:00}"
            : $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    public static string NewFormatted(this TimeMs timeMs)
    {
        var ts = timeMs.TimeStamp();

        return ts.Hours == 0 ? $"{ts.Minutes:00}:{ts.Seconds:00}" : $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    public static double ToDip(this TimeMs timeMs, TimeMs duration, double width) =>
        timeMs.Value * width / Math.Max(1, duration.Value);
}