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
using Serilog;

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

    private readonly DispatcherTimer _debounceTimer;
    private double _currentPlayHeadPosition;

    public PlayHeadControl()
    {
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _debounceTimer.Tick += (s, e) =>
        {
            _debounceTimer.Stop();
            CurrentTimePosition = TimeMs.FromDip(_currentPlayHeadPosition, Bounds.Width, MediaDuration);
        };
        
        AffectsRender<PlayHeadControl>(MediaDurationProperty);
    }
    
    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, Bounds.Width, PlayHeadWidth));

        var rectOriginX = _currentPlayHeadPosition - PlayHeadWidth / 2;

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

        context.DrawLine(new Pen(Brushes.Red, 2), new Point(_currentPlayHeadPosition, 0), new Point(_currentPlayHeadPosition, Bounds.Height));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var position = e.GetPosition(this);

        if (_playHead.TryPress(position, new Rect(0, 0, Bounds.Width, PlayHeadWidth)))
        {
            _currentPlayHeadPosition = position.X;
            _debounceTimer.Stop();
            _debounceTimer.Start();
            InvalidateVisual();
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        var rectOriginX = position.X - PlayHeadWidth / 2;
        var rect = new Rect(rectOriginX, 0, PlayHeadWidth, PlayHeadWidth);

        Cursor = rect.Contains(position) ? Cursor.Parse("Hand") : Cursor.Default;

        if (_playHead.TryDrag())
        {
            _currentPlayHeadPosition = position.X;
            _debounceTimer.Stop();
            _debounceTimer.Start();
            InvalidateVisual();
        }

        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        CurrentTimePosition = TimeMs.FromDip(_currentPlayHeadPosition, Bounds.Width, MediaDuration);
        _playHead.Release();
        
        base.OnPointerReleased(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property.Name is nameof(CurrentTimePosition) && !_playHead.Pressed)
        {
            _currentPlayHeadPosition = CurrentTimePosition.ToDip(MediaDuration, Bounds.Width);
            InvalidateVisual();
        }
        
        base.OnPropertyChanged(change);
    }
}