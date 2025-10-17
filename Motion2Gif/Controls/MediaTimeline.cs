using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
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

public class MediaTimeline : Control
{
    public static readonly DirectProperty<MediaTimeline, TimeMs> CurrentPositionProperty =
        AvaloniaProperty.RegisterDirect<MediaTimeline, TimeMs>(
            nameof(CurrentPosition),
            o => o.CurrentPosition,
            (o, v) => o.CurrentPosition = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: false
            );
    
    private TimeMs _currentPosition;

    public TimeMs CurrentPosition
    {
        get => _currentPosition;
        set => SetAndRaise(CurrentPositionProperty, ref _currentPosition, value);
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
    
    public MediaTimeline()
    {
        AffectsRender<MediaTimeline>(CurrentPositionProperty, MediaDurationProperty);
    }

    private const float height = 35f;
    private PositionMarker _positionMarker = new PositionMarker(new Rect(0, 0, 5, height));
    
    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Gray, Bounds);

        var y = height / 2f;
        var pointZero = new Point(0, y);
        
        // line
        context.DrawLine(new Pen(Brushes.Beige, height), pointZero, new Point(Bounds.Width, y));

        _positionMarker = GetPositionMarker();
        
        // progress
        context.DrawLine(new Pen(Brushes.GreenYellow, height), pointZero, _positionMarker.Box.Center);
        
        // marker
        context.DrawRectangle(Brushes.Red, null, _positionMarker.Box);
    }

    private PositionMarker GetPositionMarker()
    {
        var markerPosition = CurrentPosition.Value * Bounds.Width / Math.Max(1, MediaDuration.Value);
        return new PositionMarker(new Rect(markerPosition, 0, 5, height));        
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        CurrentPosition = new TimeMs(3000);
        Log.Information("OnPointerPressed");
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        Log.Information("OnPointerMoved");
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        Log.Information("OnPointerReleased");
        base.OnPointerReleased(e);
    }
}
