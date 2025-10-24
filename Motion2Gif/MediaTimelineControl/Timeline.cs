using Avalonia;

namespace Motion2Gif.MediaTimelineControl;

public record struct Timeline()
{
    private bool _pressed = false;
    
    public bool TryPress(Point point, Rect rect)
    {
        _pressed = rect.Contains(point);
        return _pressed;
    }

    public bool TryMove() => _pressed;

    public void Release() => _pressed = false;
}