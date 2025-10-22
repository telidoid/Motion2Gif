using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Motion2Gif.Controls;
using Motion2Gif.Other;

namespace Motion2Gif.MediaTimelineControl;

// ReSharper disable once MemberCanBePrivate.
public class MediaTimeline : Control
{
    #region Properties

    public static readonly DirectProperty<MediaTimeline, TimeMs> CurrentTimePositionProperty =
        AvaloniaProperty.RegisterDirect<MediaTimeline, TimeMs>(
            nameof(CurrentTimePosition),
            o => o.CurrentTimePosition,
            (o, v) => o.CurrentTimePosition = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: false
            );

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

    public TimeMs MediaDuration
    {
        get => _mediaDuration;
        set => SetAndRaise(MediaDurationProperty, ref _mediaDuration, value);
    }
    
    #endregion
    
    private TimeMs _currentTimePosition;
    private TimeMs _mediaDuration;
    private Timeline _timeline;
    
    public MediaTimeline()
    {
        AffectsRender<MediaTimeline>(CurrentTimePositionProperty, MediaDurationProperty);
        _timeline =  new Timeline(new Rect(0, 0, Bounds.Width, Bounds.Height));
    }
    
    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Gray, Bounds);
        
        _timeline = _timeline with { Box = new Rect(0, 0, Bounds.Width, Bounds.Height )};
        context.DrawRectangle(Brushes.Beige, null, _timeline.Box);

        var marker = GetPositionMarker();
        var pointZero = new Point(0, Bounds.Height / 2f);
        context.DrawLine(new Pen(Brushes.GreenYellow, Bounds.Height), pointZero, marker.Box.Center);
        context.DrawRectangle(Brushes.Red, null, marker.Box);
    }

    private PositionMarker GetPositionMarker()
    {
        var nextXPos = CurrentTimePosition.ToDip(MediaDuration, Bounds.Width);
        
        if (CurrentTimePosition == MediaDuration && CurrentTimePosition is not {Value: 0})
            nextXPos = Bounds.Width;

        var width = 1;
        
        return new PositionMarker(new Rect(nextXPos-(width/2f), 0, width, Bounds.Height));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);

        if (_timeline.TryPress(point.Position))
        {
            e.Pointer.Capture(this);
            CurrentTimePosition = TimeMs.FromDip(point.Position.X, Bounds.Width, MediaDuration);
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        
        if (_timeline.TryMove(point.Position))
        {
            e.Pointer.Capture(this);
            CurrentTimePosition = TimeMs.FromDip(point.Position.X, Bounds.Width, MediaDuration);
        }

        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
        _timeline.Release();
        base.OnPointerReleased(e);
    }
}
