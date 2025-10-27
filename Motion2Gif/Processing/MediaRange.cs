using System;
using Motion2Gif.Player;

namespace Motion2Gif.Processing;

public record MediaRange(TimeMs Start, TimeMs End)
{
    public static MediaRange Create(TimeMs start, TimeMs end)
    {
        if (end < start)
            throw new ArgumentException("end must be greater than start");

        return new MediaRange(start, end);
    }
}

public static class MediaRangeExtensions
{
    public static TimeMs GetDuration(this MediaRange mediaRange)
    {
        long ticks = mediaRange.End.TimeSpan().Ticks - mediaRange.Start.TimeSpan().Ticks;
        long ms = (ticks + 5_000) / 10_000; // округление к ближайшему мс
        return new TimeMs(ms);
    }
}