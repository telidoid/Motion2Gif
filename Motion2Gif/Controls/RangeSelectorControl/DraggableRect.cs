using Avalonia;

namespace Motion2Gif.Controls.RangeSelectorControl;

public record DraggableRect
{
    private bool _pressed;

    public bool TryPress(Point point, Rect rect)
    {
        _pressed = rect.Contains(point);
        return _pressed;
    }

    public bool TryDrag() => _pressed;
    
    public void Release() => _pressed = false;
}