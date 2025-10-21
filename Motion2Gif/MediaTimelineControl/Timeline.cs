using Avalonia;

namespace Motion2Gif.MediaTimelineControl;

public record struct Timeline(Rect Box)
{
    private bool _pressed = false;

    public bool TryPress(Point point)
    {
        if (!Box.Contains(point)) return false;
        _pressed = true;
        return true;
    }

    public bool TryMove(Point point) => _pressed;

    public void Release() => _pressed = false;
}