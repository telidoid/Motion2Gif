using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace Motion2Gif.Controls;

// ReSharper disable once MemberCanBePrivate.
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
        var nextXPos = CurrentTimePosition.Value * Bounds.Width / Math.Max(1, MediaDuration.Value);
        
        if (CurrentTimePosition == MediaDuration && CurrentTimePosition is not {Value: 0})
            nextXPos = Bounds.Width;

        var width = 5;
        
        return new PositionMarker(new Rect(nextXPos-(width/2f), 0, width, height));
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

        _timeline.Unpressed(point.Position);
        
        base.OnPointerReleased(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        _timeline.Release();
        base.OnPointerExited(e);
    }
}
