using Avalonia;

namespace Motion2Gif.Controls;

public record struct PositionMarker(Rect Box)
{
    public bool IsHit(Point point)
    {
        return Box.Contains(point);
    }
}