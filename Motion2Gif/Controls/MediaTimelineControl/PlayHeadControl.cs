using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Motion2Gif.Controls.RangeSelectorControl;
using Motion2Gif.Player;

namespace Motion2Gif.Controls.MediaTimelineControl;

public class PlayHeadControl : Control
{
    #region Properties

    public static readonly DirectProperty<PlayHeadControl, TimeMs> CurrentTimePositionProperty =
        AvaloniaProperty.RegisterDirect<PlayHeadControl, TimeMs>(
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

    public static readonly DirectProperty<PlayHeadControl, TimeMs> MediaDurationProperty =
        AvaloniaProperty.RegisterDirect<PlayHeadControl, TimeMs>(
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

    private const float PlayHeadWidth = 15f;
    private readonly DraggableRect _playHead = new();
    
    public PlayHeadControl()
    {
        CurrentTimePosition = new TimeMs(10_000);
        MediaDuration = new TimeMs(500_000_000);
        AffectsRender<PlayHeadControl>(MediaDurationProperty, CurrentTimePositionProperty);
    }
    
    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, Bounds.Width, PlayHeadWidth));

        var dip = CurrentTimePosition.ToDip(MediaDuration, Bounds.Width);
        var rectOriginX = dip - PlayHeadWidth / 2;

        var geometry = new StreamGeometry();
        using (var c = geometry.Open())
        {
            c.BeginFigure(new Point(rectOriginX, 0), true);
            c.LineTo(new Point(rectOriginX + PlayHeadWidth, 0), true);
            c.LineTo(new Point(rectOriginX + PlayHeadWidth / 2, PlayHeadWidth));
            c.EndFigure(true);
        }
        context.DrawGeometry(Brushes.Red, null, geometry);

        // temporary decision to scroll
        var rect = new Rect(rectOriginX, 0, PlayHeadWidth, PlayHeadWidth);
        Dispatcher.UIThread.Post(() => this.BringIntoView(rect));

        context.DrawLine(new Pen(Brushes.Red, 2), new Point(dip, 0), new Point(dip, Bounds.Height));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var position = e.GetPosition(this);

        if (_playHead.TryPress(position, new Rect(0, 0, Bounds.Width, PlayHeadWidth)))
            CurrentTimePosition = TimeMs.FromDip(position.X, Bounds.Width, MediaDuration);

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        var rectOriginX = position.X - PlayHeadWidth / 2;
        var rect = new Rect(rectOriginX, 0, PlayHeadWidth, PlayHeadWidth);

        Cursor = rect.Contains(position) ? Cursor.Parse("Hand") : Cursor.Default;

        if (_playHead.TryDrag())
            CurrentTimePosition = TimeMs.FromDip(position.X, Bounds.Width, MediaDuration);
        
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _playHead.Release();
        base.OnPointerReleased(e);
    }
}