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
    }
    
    public override void Render(DrawingContext context)
    {
        var marker = GetPositionMarker();
        context.DrawRectangle(Brushes.White, null, new Rect(0, 0, Bounds.Width, Bounds.Height));
        context.DrawRectangle(Brushes.Green, null, new Rect(0, 0, marker.Box.Left, Bounds.Height));
        context.DrawRectangle(Brushes.Red, null, marker.Box);
    }

    private PositionMarker GetPositionMarker()
    {
        var nextXPos = CurrentTimePosition.ToDip(MediaDuration, Bounds.Width);
        
        if (CurrentTimePosition == MediaDuration && CurrentTimePosition is not {Value: 0})
            nextXPos = Bounds.Width;

        const int width = 1;
        
        return new PositionMarker(new Rect(nextXPos, 0, width, Bounds.Height));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var position = e.GetPosition(this);

        if (_timeline.TryPress(position, new Rect(0, 0, Bounds.Width, Bounds.Height)))
            CurrentTimePosition = TimeMs.FromDip(position.X, Bounds.Width, MediaDuration);

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        
        if (_timeline.TryMove())
            CurrentTimePosition = TimeMs.FromDip(position.X, Bounds.Width, MediaDuration);

        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _timeline.Release();
        
        base.OnPointerReleased(e);
    }
}
