using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Serilog;

namespace Motion2Gif.Controls;

// ReSharper disable once MemberCanBePrivate.

public record struct PositionMarker(Rect Box)
{
    public bool IsHit(Point point)
    {
        return Box.Contains(point);
    }
}

public record struct Timeline(Rect Box)
{
    private bool _isPressed = false;

    public void Pressed(Point pointPosition, Action action)
    {
        if (!Box.Contains(pointPosition)) 
            return;

        _isPressed = true;
        action();
    }

    public void Moved(Point pointPosition, Action action)
    {
        if (!_isPressed)
            return;

        if (!Box.Contains(pointPosition)) 
            return;

        action();
    }

    public void Unpressed(Point pointPosition)
    {
        if (!_isPressed)
            return;

        if (!Box.Contains(pointPosition)) 
            return;

        _isPressed = false;
    }
}

public class MediaTimeline : Control
{
    public static readonly DirectProperty<MediaTimeline, TimeMs> CurrentTimePositionProperty =
        AvaloniaProperty.RegisterDirect<MediaTimeline, TimeMs>(
            nameof(CurrentTimePosition),
            o => o.CurrentTimePosition,
            (o, v) => o.CurrentTimePosition = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: false
            );
    
    private TimeMs _currentTimePosition;

    public TimeMs CurrentTimePosition
    {
        get => _currentTimePosition;
        set => SetAndRaise(CurrentTimePositionProperty, ref _currentTimePosition, value);
    }

    public static readonly DirectProperty<MediaTimeline, TimeMs> MediaDurationProperty =
        AvaloniaProperty.RegisterDirect<MediaTimeline, TimeMs>(
            nameof(MediaDuration),
            o => o.MediaDuration,
            (o, v) => o.MediaDuration = v,
            defaultBindingMode: BindingMode.OneWay,
            enableDataValidation: false
            );

    private TimeMs _mediaDuration;

    public TimeMs MediaDuration
    {
        get => _mediaDuration;
        set => SetAndRaise(MediaDurationProperty, ref _mediaDuration, value);
    }
    
    private const float height = 35f;
    private Timeline _timeline;
    
    public MediaTimeline()
    {
        _timeline =  new Timeline(new Rect(0, 0, Bounds.Width, height));
        
        AffectsRender<MediaTimeline>(CurrentTimePositionProperty, MediaDurationProperty);
    }
    
    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Gray, Bounds);
        
        _timeline = _timeline with { Box = new Rect(0, 0, Bounds.Width, height )};
        context.DrawRectangle(Brushes.Beige, null, _timeline.Box);

        var marker = GetPositionMarker();
        var pointZero = new Point(0, height / 2f);
        context.DrawLine(new Pen(Brushes.GreenYellow, height), pointZero, marker.Box.Center); // progression
        context.DrawRectangle(Brushes.Red, null, marker.Box); // marker
    }

    private PositionMarker GetPositionMarker()
    {
        var markerPosition = CurrentTimePosition.Value * Bounds.Width / Math.Max(1, MediaDuration.Value);
        
        if (CurrentTimePosition == MediaDuration && CurrentTimePosition is not {Value: 0})
            markerPosition = Bounds.Width;

        var width = 5;
        
        return new PositionMarker(new Rect(markerPosition-width, 0, width, height));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        
        _timeline.Pressed(point.Position, () =>
        {
            CurrentTimePosition = TimeMs.FromDip(point.Position.X, Bounds.Width, MediaDuration);
        });

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        
        _timeline.Moved(point.Position, () =>
        {
            CurrentTimePosition = TimeMs.FromDip(point.Position.X, Bounds.Width, MediaDuration);
        });

        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        Log.Information($"OnPointerReleased, {point.Position}");

        _timeline.Unpressed(point.Position);
        
        base.OnPointerReleased(e);
    }
}
