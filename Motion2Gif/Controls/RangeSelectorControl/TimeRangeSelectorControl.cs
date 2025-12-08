using System;
using System.Reflection.Metadata.Ecma335;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using Motion2Gif.Player;
using Serilog;

namespace Motion2Gif.Controls.RangeSelectorControl;

public class TimeRangeSelectorControl : Control
{
    #region Properties

    public static readonly DirectProperty<TimeRangeSelectorControl, TimeMs> TrimOriginProperty =
        AvaloniaProperty.RegisterDirect<TimeRangeSelectorControl, TimeMs>(
            nameof(TrimOrigin),
            o => o.TrimOrigin,
            (o, v) => o.TrimOrigin = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: false
        );

    public TimeMs TrimOrigin
    {
        get => _trimOrigin;
        set => SetAndRaise(TrimOriginProperty, ref _trimOrigin, value);
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

    private TimeMs _trimOrigin;
    private TimeMs _trimEnd;
    private TimeMs _min;
    private TimeMs _max;

    private readonly DraggableRect _leftHandle = new();
    private readonly DraggableRect _rightHandle = new();
    private readonly DraggableRect _createBox = new();
    private readonly DraggableRect _boxItself = new();
    private double _distanceFromStart;
    private double _distanceToEnd;

    private const double RectWidth = 4;
    private const long MinimumTimeRangeBetweenHandles = 100; // in ms

    private static readonly Cursor ResizeEwCursor = new(StandardCursorType.SizeWestEast);
    private static readonly Cursor DefaultCursor = new(StandardCursorType.Arrow);
    private static readonly Cursor PointerCursor = new(StandardCursorType.Hand);

    private DipRange _dipRange = new();

    public TimeRangeSelectorControl()
    {
        AffectsRender<TimeRangeSelectorControl>(MaxProperty, MinProperty, TrimOriginProperty, TrimEndProperty);

        MaxProperty.Changed.AddClassHandler<TimeRangeSelectorControl>((s, e) =>
        {
            TrimOrigin = new TimeMs(0);
            TrimEnd = new TimeMs(0 + MinimumTimeRangeBetweenHandles);
        });
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, Bounds.Width, Bounds.Height));

        var normalized = _dipRange.Normalize();
        var rect = new Rect(normalized.Origin, 0, normalized.Width, Bounds.Height);

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
        var rect = new Rect(_dipRange.Origin, 0, _dipRange.Width, Bounds.Height);

        if (_boxItself.TryPress(point, rect))
        {
            _distanceFromStart = point.X - _dipRange.Origin;
            _distanceToEnd = _dipRange.End - point.X;
        }
        else if (_createBox.TryPress(point, new Rect(0, 0, Bounds.Width, Bounds.Height)))
        {
            _dipRange = new DipRange(point.X, point.X);
        }

        InvalidateVisual();
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var point = e.GetPosition(this);

        if (_boxItself.TryDrag())
            _dipRange = new DipRange(point.X - _distanceFromStart, point.X + _distanceToEnd);

        if (_createBox.TryDrag())
            _dipRange = _dipRange with { End = point.X };

        InvalidateVisual();
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

    private void UpdateTimeMsRange()
    {
    }
}

public readonly record struct DipRange(double Origin, double End)
{
    public DipRange Normalize() => new(Math.Min(Origin, End), Math.Max(Origin, End));
    public double Width => End - Origin;
}

public record struct TimeMsRange(TimeMs Origin, TimeMs End);

public static class DipRangeExtensions
{
    extension(DipRange dipRange)
    {
        public TimeMsRange ToTimeMsRange(double width, TimeMs duration)
        {
            var start = dipRange.Origin;
            var end = dipRange.End;

            if (start > end)
                (start, end) = (end, start);

            return new TimeMsRange(
                TimeMs.FromDip(start, width, duration),
                TimeMs.FromDip(end, width, duration)
            );
        }
    }
}