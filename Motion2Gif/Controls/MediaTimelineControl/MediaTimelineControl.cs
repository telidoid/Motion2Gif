using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Motion2Gif.Player;

namespace Motion2Gif.Controls.MediaTimelineControl;

// ReSharper disable once MemberCanBePrivate.
public class MediaTimelineControl : Control
{
    #region Properties

    public static readonly DirectProperty<MediaTimelineControl, TimeMs> CurrentTimePositionProperty =
        AvaloniaProperty.RegisterDirect<MediaTimelineControl, TimeMs>(
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

    public static readonly DirectProperty<MediaTimelineControl, TimeMs> MediaDurationProperty =
        AvaloniaProperty.RegisterDirect<MediaTimelineControl, TimeMs>(
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

    public MediaTimelineControl()
    {
        AffectsRender<MediaTimelineControl>(CurrentTimePositionProperty, MediaDurationProperty);
    }

    public override void Render(DrawingContext context)
    {
        var nextXPos = CurrentTimePosition == MediaDuration && CurrentTimePosition is not { Value: 0 }
            ? Bounds.Width
            : CurrentTimePosition.ToDip(MediaDuration, Bounds.Width);

        context.DrawRectangle(Brushes.White, null, new Rect(0, 0, Bounds.Width, Bounds.Height));
        context.DrawRectangle(Brushes.Green, null, new Rect(0, 0, nextXPos, Bounds.Height));
    }
}