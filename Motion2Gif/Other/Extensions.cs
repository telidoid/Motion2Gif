using System;

namespace Motion2Gif.Other;

public static class Extensions
{
    public static string ToDurationText(this long durationInMs)
    {
        var ts = TimeSpan.FromMilliseconds(durationInMs);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }
}