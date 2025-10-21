using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Motion2Gif.Other;


namespace Motion2Gif.RangeSelectorControl;

public class TimeRangeSelector : Control
{
    #region Properties

    public static readonly DirectProperty<TimeRangeSelector, TimeMs> TrimStartProperty =
        AvaloniaProperty.RegisterDirect<TimeRangeSelector, TimeMs>(
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

    public static readonly DirectProperty<TimeRangeSelector, TimeMs> TrimEndProperty =
        AvaloniaProperty.RegisterDirect<TimeRangeSelector, TimeMs>(
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

    public static readonly DirectProperty<TimeRangeSelector, TimeMs> MinProperty =
        AvaloniaProperty.RegisterDirect<TimeRangeSelector, TimeMs>(
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

    public static readonly DirectProperty<TimeRangeSelector, TimeMs> MaxProperty =
        AvaloniaProperty.RegisterDirect<TimeRangeSelector, TimeMs>(
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

    private const double RectWidth = 10;
    private const double RectHeight = 35;
    private const long MinimumTimeRangeBetweenHandles = 1_000; // in ms

    public TimeRangeSelector()
    {
        AffectsRender<TimeRangeSelector>(MaxProperty, MinProperty, TrimStartProperty, TrimEndProperty);

        MaxProperty.Changed.AddClassHandler<TimeRangeSelector>((s, e) =>
        {
            TrimStart = new TimeMs(0);
            TrimEnd = new TimeMs(0 + MinimumTimeRangeBetweenHandles);
        });
    }
    
    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Blue, null, new Rect(TrimStart.ToDip(Max, Bounds.Width), 0, RectWidth, RectHeight));
        context.DrawRectangle(Brushes.Black, null, new Rect(TrimEnd.ToDip(Max, Bounds.Width), 0, RectWidth, RectHeight));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);

        if (!_leftHandle.TryPress(point, new Rect(TrimStart.ToDip(Max, Bounds.Width), 0, RectWidth, RectHeight)))
            _rightHandle.TryPress(point, new Rect(TrimEnd.ToDip(Max, Bounds.Width), 0, RectWidth, RectHeight));
        
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var point = e.GetPosition(this);

        if (_leftHandle.TryDrag())
        {
            var newTimePosition = TimeMs.FromDip(point.X - RectWidth/2, Bounds.Width, Max);

            if (newTimePosition < new TimeMs(Value: TrimEnd.Value - MinimumTimeRangeBetweenHandles))
                TrimStart = newTimePosition;
        }
        else if (_rightHandle.TryDrag())
        {
            var newTimePosition = TimeMs.FromDip(point.X - RectWidth/2, Bounds.Width, Max);

            if (newTimePosition > new TimeMs(Value: TrimStart.Value + MinimumTimeRangeBetweenHandles))
                TrimEnd = newTimePosition;
        }
        
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _leftHandle.Release();
        _rightHandle.Release();
        
        base.OnPointerReleased(e);
    }
}