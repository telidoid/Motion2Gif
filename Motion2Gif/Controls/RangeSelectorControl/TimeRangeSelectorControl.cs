using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Motion2Gif.Player;

namespace Motion2Gif.Controls.RangeSelectorControl;

public class TimeRangeSelectorControl : Control
{
    #region Properties

    public static readonly DirectProperty<TimeRangeSelectorControl, TimeMs> TrimStartProperty =
        AvaloniaProperty.RegisterDirect<TimeRangeSelectorControl, TimeMs>(
            nameof(TrimStart),
            o => o.TrimStart,
            (o, v) => o.TrimStart = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: false
        );

    public TimeMs TrimStart
    {
        get => _trimStart;
        set => SetAndRaise(TrimStartProperty, ref _trimStart, value);
    }

    public static readonly DirectProperty<TimeRangeSelectorControl, TimeMs> TrimEndProperty =
        AvaloniaProperty.RegisterDirect<TimeRangeSelectorControl, TimeMs>(
            nameof(TrimEnd),
            o => o.TrimEnd,
            (o, v) => o.TrimEnd = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: false
        );

    public TimeMs TrimEnd
    {
        get => _trimEnd;
        set => SetAndRaise(TrimEndProperty, ref _trimEnd, value);
    }

    public static readonly DirectProperty<TimeRangeSelectorControl, TimeMs> MinProperty =
        AvaloniaProperty.RegisterDirect<TimeRangeSelectorControl, TimeMs>(
            nameof(Min),
            o => o.Min,
            (o, v) => o.Min = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: false);

    public TimeMs Min
    {
        get => _min;
        set => SetAndRaise(MinProperty, ref _min, value);
    }

    public static readonly DirectProperty<TimeRangeSelectorControl, TimeMs> MaxProperty =
        AvaloniaProperty.RegisterDirect<TimeRangeSelectorControl, TimeMs>(
            nameof(Max),
            o => o.Max,
            (o, v) => o.Max = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: false);

    public TimeMs Max
    {
        get => _max;
        set => SetAndRaise(MaxProperty, ref _max, value);
    }
    
    #endregion

    private TimeMs _trimStart;
    private TimeMs _trimEnd;
    private TimeMs _min;
    private TimeMs _max;

    private readonly DraggableRect _leftHandle = new();
    private readonly DraggableRect _rightHandle = new();

    private const double RectWidth = 4;
    private const long MinimumTimeRangeBetweenHandles = 100; // in ms
    
    private static readonly Cursor ResizeEwCursor = new(StandardCursorType.SizeWestEast);
    private static readonly Cursor DefaultCursor  = new(StandardCursorType.Arrow);
    private static readonly Cursor PointerCursor  = new(StandardCursorType.Hand);

    public TimeRangeSelectorControl()
    {
        AffectsRender<TimeRangeSelectorControl>(MaxProperty, MinProperty, TrimStartProperty, TrimEndProperty);

        MaxProperty.Changed.AddClassHandler<TimeRangeSelectorControl>((s, e) =>
        {
            TrimStart = new TimeMs(0);
            TrimEnd = new TimeMs(0 + MinimumTimeRangeBetweenHandles);
        });
    }
    
    public override void Render(DrawingContext context)
    {
        var yellowBrush = new SolidColorBrush(Colors.Yellow, 1);
        var brush3 = new SolidColorBrush(Colors.Aqua, 0.4);

        var leftHandle = this.GetLeftRect();
        var rightHandle = this.GetRightRect();
        
        context.DrawRectangle(yellowBrush, null, leftHandle);
        context.DrawRectangle(yellowBrush, null, rightHandle);
        context.DrawRectangle(brush3, null, new Rect(leftHandle.BottomLeft.X, 0, rightHandle.Right - leftHandle.Right, Bounds.Height));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);
        
        if (!_leftHandle.TryPress(point, this.GetLeftRect()))
            _rightHandle.TryPress(point, this.GetRightRect());
        
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var point = e.GetPosition(this);

        if (_leftHandle.TryDrag())
        {
            var newTimePosition = TimeMs.FromDip(point.X - RectWidth/2 + RectWidth, Bounds.Width, Max);

            if (newTimePosition < new TimeMs(Value: TrimEnd.Value - MinimumTimeRangeBetweenHandles))
                TrimStart = newTimePosition;
            
            Cursor = ResizeEwCursor;
        }
        else if (_rightHandle.TryDrag())
        {
            var newTimePosition = TimeMs.FromDip(point.X - RectWidth/2, Bounds.Width, Max);

            if (newTimePosition > new TimeMs(Value: TrimStart.Value + MinimumTimeRangeBetweenHandles))
                TrimEnd = newTimePosition;
            
            Cursor = ResizeEwCursor;
        }
        
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _leftHandle.Release();
        _rightHandle.Release();
        
        var point = e.GetPosition(this);
        
        Cursor = GetLeftRect().Contains(point) || GetRightRect().Contains(point)
            ? PointerCursor
            : DefaultCursor;
        
        base.OnPointerReleased(e);
    }
    
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        var point = e.GetPosition(this);
        
        Cursor = GetLeftRect().Contains(point) || GetRightRect().Contains(point)
            ? PointerCursor
            : DefaultCursor;
        
        base.OnPointerEntered(e);
    }
    
    protected override void OnPointerExited(PointerEventArgs e)
    {
        Cursor = DefaultCursor;
        base.OnPointerExited(e);
    }
    
    private Rect GetLeftRect() => new(TrimStart.ToDip(Max, Bounds.Width) - RectWidth, 0, RectWidth, Bounds.Height);

    private Rect GetRightRect() => new(TrimEnd.ToDip(Max, Bounds.Width), 0, RectWidth, Bounds.Height);
}