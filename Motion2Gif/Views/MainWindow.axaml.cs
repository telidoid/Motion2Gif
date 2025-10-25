using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Motion2Gif.Other;
using Motion2Gif.ViewModels;

namespace Motion2Gif.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Core.Initialize();
        
        var filePickerService = new FilePickerService(() => this);
        DataContext = new MainWindowViewModel(filePickerService);
        
        Dispatcher.UIThread.Post(() =>
        {
            var vm = DataContext as MainWindowViewModel;
            vm!.PlayerService.AttachPlayer(VideoView);
        });
        
        this.Closed += OnWindowClosed;
    }

    private void OnWindowClosed(object? o, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // _player.Dispose();
            // _libVlc.Dispose();
        });
        
        this.Closed -= OnWindowClosed;
    }

    private void ZoomIn_OnClick(object? sender, RoutedEventArgs e)
    {
        var windowWidth = this.Bounds.Width - (TimelineContainer.Margin.Right + TimelineContainer.Margin.Left);
        
        if (TimelineGrid.MinWidth + 200 < windowWidth)
        {
            TimelineGrid.MinWidth = windowWidth + 200;
        }
        else
        {
            TimelineGrid.MinWidth += 200;
        }
    }

    private void ZoomOut_OnClick(object? sender, RoutedEventArgs e)
    {
        var windowWidth = this.Bounds.Width - (TimelineContainer.Margin.Right + TimelineContainer.Margin.Left);
        
        if (TimelineGrid.MinWidth - 200 < windowWidth)
        {
            TimelineGrid.MinWidth = windowWidth;
        }
        else
        {
            TimelineGrid.MinWidth -= 200;
        }
    }

    private void AutofitWidth_OnClick(object? sender, RoutedEventArgs e)
    {
        var windowWidth = this.Bounds.Width - (TimelineContainer.Margin.Right + TimelineContainer.Margin.Left);
        TimelineGrid.MinWidth = windowWidth;
    }
}