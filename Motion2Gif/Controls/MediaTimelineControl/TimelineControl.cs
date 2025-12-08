using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Motion2Gif.Player;

namespace Motion2Gif.Controls.MediaTimelineControl;

public class TimelineControl : Control
{
    #region Properties

    public static readonly DirectProperty<TimelineControl, TimeMs> CurrentTimePositionProperty =
        AvaloniaProperty.RegisterDirect<TimelineControl, TimeMs>(
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

    public static readonly DirectProperty<TimelineControl, TimeMs> MediaDurationProperty =
        AvaloniaProperty.RegisterDirect<TimelineControl, TimeMs>(
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

    private const double TimeScaleHeight = 15;
    private const double StepInDip = 100;
    private List<TimeMs> _timePoints = [];

    public TimelineControl()
    {
        AffectsRender<TimelineControl>(MediaDurationProperty);
        CurrentTimePosition = new TimeMs(10_000);
        MediaDuration = new TimeMs(500_000_000);
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.White, null, new Rect(0, 0, Bounds.Width, TimeScaleHeight));

        foreach (var timePoint in _timePoints)
            DrawTimePoint(context, timePoint);

        base.Render(context);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var propertyChanged = change.Property.Name is nameof(MediaDuration) or nameof(Bounds);

        if (propertyChanged)
            UpdateTimeline();

        base.OnPropertyChanged(change);
    }

    private void UpdateTimeline()
    {
        _timePoints = [];

        if (MediaDuration.Value == 0)
            return;

        var stepInMs = TimeMs.FromDip(StepInDip, Bounds.Width, MediaDuration);
        var divisionCount = Bounds.Width / StepInDip;

        for (var i = 1; i < divisionCount; i++)
            _timePoints.Add(new TimeMs(stepInMs.Value * i));
    }

    private void DrawTimePoint(DrawingContext context, TimeMs timePoint)
    {
        var text = new FormattedText(
            timePoint.NewFormatted(),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            12,
            Brushes.Black);
        
        var dip = timePoint.ToDip(MediaDuration, Bounds.Width);
        var x = Math.Round(dip - text.Width / 2.0);
        
        context.DrawText(text, new Point(x, 0));
    }
}