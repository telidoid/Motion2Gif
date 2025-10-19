using System;

namespace Motion2Gif.Controls;

public record struct TimeMs(long Value)
{
    public static TimeMs FromDip(double value, double length, TimeMs timeMs)
    {
        var totalMs = Math.Max(0L, timeMs.Value); // защита от нулей/отрицательных
        if (length <= 0 || totalMs == 0)
            return new TimeMs(0);
        
        var ratio = value / length; // доля пройденного пути по ширине [0..1]
        var ms = Math.Clamp(ratio, 0.0, 1.0) * totalMs; // кламп доли и перевод в миллисекунды

        return new TimeMs((long)Math.Round(ms));
    }
}

public static class TimeMsExtensions
{
    private static TimeSpan TimeStamp(this TimeMs time) => TimeSpan.FromMilliseconds(time.Value);
    
    public static string Formatted(this TimeMs timeMs)
    {
        var ts = timeMs.TimeStamp();
        return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }
}