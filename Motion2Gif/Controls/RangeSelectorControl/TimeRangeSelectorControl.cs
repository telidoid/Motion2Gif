using System.Reflection.Metadata.Ecma335;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Motion2Gif.Player;
using Serilog;

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
    private readonly DraggableRect _createBox = new();
    private readonly DraggableRect _boxItself = new();
    private TimeMs _distanceFromStart;
    private TimeMs _distanceToEnd;

    private const double RectWidth = 4;
    private const long MinimumTimeRangeBetweenHandles = 100; // in ms

    private static readonly Cursor ResizeEwCursor = new(StandardCursorType.SizeWestEast);
    private static readonly Cursor DefaultCursor = new(StandardCursorType.Arrow);
    private static readonly Cursor PointerCursor = new(StandardCursorType.Hand);

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
        context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, Bounds.Width, Bounds.Height));

        var startDip = TrimStart.ToDip(Max, Bounds.Width);
        var endDip = TrimEnd.ToDip(Max, Bounds.Width);
        var rect = new Rect(startDip, 0, endDip - startDip, Bounds.Height);

        context.DrawRectangle(new SolidColorBrush(Colors.Aqua, 0.4), null, rect);

        context.DrawLine(new Pen(Brushes.OrangeRed, 2, DashStyle.Dash),
            new Point(rect.BottomLeft.X, rect.BottomLeft.Y),
            new Point(rect.TopLeft.X, rect.TopLeft.Y));

        context.DrawLine(new Pen(Brushes.OrangeRed, 2, DashStyle.Dash),
            new Point(rect.BottomRight.X, rect.BottomRight.Y),
            new Point(rect.TopRight.X, rect.TopRight.Y));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);

        var startDip = TrimStart.ToDip(Max, Bounds.Width);
        var endDip = TrimEnd.ToDip(Max, Bounds.Width);
        var rect = new Rect(startDip, 0, endDip - startDip, Bounds.Height);

        if (_boxItself.TryPress(point, rect))
        {
            _distanceFromStart = new TimeMs(TimeMs.FromDip(point.X, Bounds.Width, Max).Value - TrimStart.Value);
            _distanceToEnd = new TimeMs(TrimEnd.Value - TimeMs.FromDip(point.X, Bounds.Width, Max).Value);
        }
        else if (_createBox.TryPress(point, new Rect(0, 0, Bounds.Width, Bounds.Height)))
        {
            TrimStart = TimeMs.FromDip(point.X, Bounds.Width, Max);
            TrimEnd = TimeMs.FromDip(point.X, Bounds.Width, Max);
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var point = e.GetPosition(this);

        if (_boxItself.TryDrag())
        {
            var pointMs = TimeMs.FromDip(point.X, Bounds.Width, Max);
            TrimStart = new TimeMs(pointMs.Value - _distanceFromStart.Value);
            TrimEnd = new TimeMs(pointMs.Value + _distanceToEnd.Value);
        }

        if (_createBox.TryDrag())
        {
            var pointMs = TimeMs.FromDip(point.X, Bounds.Width, Max);
            _ = pointMs <= TrimStart ? TrimStart = pointMs : TrimEnd = pointMs;
        }

        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _leftHandle.Release();
        _rightHandle.Release();
        _createBox.Release();
        _boxItself.Release();

        base.OnPointerReleased(e);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        var point = e.GetPosition(this);

        base.OnPointerEntered(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        Cursor = DefaultCursor;
        base.OnPointerExited(e);
    }
}