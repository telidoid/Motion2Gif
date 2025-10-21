using Avalonia;
using Avalonia.Media;
using Serilog;

namespace Motion2Gif.RangeSelectorControl;

public record DraggableRect(Rect Geometry)
{
    private bool _pressed = false;
    
    public bool TryPress(Point point)
    {
        _pressed = Geometry.Contains(point);
        Log.Information(_pressed.ToString());
        return _pressed;
    }

    public bool TryDrag(Point point) => _pressed;
    
    public void Release() => _pressed = false;
}