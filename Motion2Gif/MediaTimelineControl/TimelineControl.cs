using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using LibVLCSharp.Shared;
using Motion2Gif.Other;
using Serilog;

namespace Motion2Gif.MediaTimelineControl;

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


    public TimelineControl()
    {
        AffectsRender<TimelineControl>(MediaDurationProperty, CurrentTimePositionProperty);
        CurrentTimePosition = new TimeMs(10_000);
        MediaDuration = new TimeMs(500_000_000);
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Aqua, null, Bounds);

        context.DrawRectangle(Brushes.White, null, new Rect(0, 0, Bounds.Width, TimeScaleHeight));

        Log.Information($"MediaDuration: {MediaDuration}, CurrentTimePosition: {CurrentTimePosition}");

        var step = Bounds.Width / 100; // 100 is 100 dip

        var currentDip = 0d;
        var timeSteps = new List<(TimeMs, double)>();
        
        while (currentDip <= Bounds.Width && MediaDuration.Value > 0)
        {
            currentDip += 100;
            var newTimeStep = TimeMs.FromDip(currentDip, Bounds.Width, MediaDuration);
            Log.Information($"timeStep: {newTimeStep}");
            timeSteps.Add((newTimeStep, currentDip));
        }

        foreach (var ts in timeSteps)
        {
            var ft = new FormattedText(
                ts.Item1.NewFormatted(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                12,
                Brushes.Red);

            var w = ft.Width;
            var x = Math.Round(ts.Item1.ToDip(MediaDuration, Bounds.Width) - w / 2.0) + 0.5;

            context.DrawText(ft, new Point(x, 0));
        }
        
        context.DrawText(
            new FormattedText(
                $"Step: {step} dip\n" +
                $"Duration: {MediaDuration.Value} ms\n" +
                $"Duration: {MediaDuration.NewFormatted()}\n" +
                $"", 
                CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 16, Brushes.Chartreuse),
            new Point(0, -100));

        base.Render(context);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
    }
}