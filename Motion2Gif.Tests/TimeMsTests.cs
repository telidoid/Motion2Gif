using Motion2Gif.Player;
using Xunit;

namespace Motion2Gif.Tests;


public class TimeMsTests
{
    [Fact(DisplayName = "ToDip: 100ms из 10s при ширине 1000 DIP → 10 DIP")]
    public void ToDip_100msOf10s_WithWidth1000_Returns10Dip()
    {
        var timeMs = new TimeMs(100);
        var duration = new TimeMs(10_000);
        var width = 1000;

        var dip = timeMs.ToDip(duration, width);

        Assert.Equal(10, dip);
    }

    [Fact(DisplayName = "Round-trip: ToDip → FromDip возвращает исходное время (простые значения)")]
    public void RoundTrip_ToDipThenFromDip_WithSimpleValues_ReturnsOriginalTime()
    {
        var timeMs = new TimeMs(100);
        var duration = new TimeMs(10_000);
        var width = 1000;

        var dip = timeMs.ToDip(duration, width);
        var timeMsFromDip = TimeMs.FromDip(dip, width, duration);

        Assert.Equal(timeMs, timeMsFromDip);
    }

    [Theory(DisplayName = "Round-trip: ToDip ↔ FromDip сохраняет время")]
    [InlineData(100,   10_000, 1000)]
    [InlineData(4452,  12_354, 854)]
    [InlineData(0,     12_354, 854)]      // крайний случай: 0 мс
    [InlineData(12_354,12_354, 854)]      // крайний случай: конец = длительности
    public void RoundTrip_ToDipThenFromDip_WithNonTrivialValues_PreservesTime(long ms, long duration, int width)
    {
        var timeMs = new TimeMs(ms);
        var dur = new TimeMs(duration);

        var dip = timeMs.ToDip(dur, width);
        var timeMsFromDip = TimeMs.FromDip(dip, width, dur);

        Assert.Equal(timeMs, timeMsFromDip);
    }
    
    [Theory(DisplayName = "Round-trip: FromDip ↔ ToDip сохраняет DIP (с учётом округления 1 мс)")]
    [InlineData(0.0,    12_354, 854)]
    [InlineData(10.0,   10_000, 1000)]
    [InlineData(127.42, 12_354, 854)]
    [InlineData(1273.42, 123_354, 3630)]
    [InlineData(853.9,  12_354, 854)]   // почти край ширины
    [InlineData(300.0,   3_000, 1000)]  // другой масштаб
    public void RoundTrip_FromDipThenToDip_PreservesDip(double dip, long duration, int width)
    {
        var dur = new TimeMs(duration);

        var t = TimeMs.FromDip(dip, width, dur);
        var dip2 = t.ToDip(dur, width);

        // 1 мс в DIP — максимальная ошибка из-за округления мс
        var eps = width / (double)duration;
        
        Assert.InRange(dip2, dip - eps, dip + eps);
    }
}