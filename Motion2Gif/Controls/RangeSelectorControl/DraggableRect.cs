using Avalonia;
using Avalonia.Controls.Shapes;

namespace Motion2Gif.Controls.RangeSelectorControl;

public record DraggableRect
{
    public bool Pressed => _pressed;
    private bool _pressed;

    public bool TryPress(Point point, Rect rect)
    {
        _pressed = rect.Contains(point);
        return _pressed;
    }

    public bool TryDrag() => _pressed;
    
    public void Release() => _pressed = false;
}