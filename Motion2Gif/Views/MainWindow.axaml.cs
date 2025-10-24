using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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
        
        ToolBar.PointerPressed += ToolBarOnPointerPressed;
        this.Closed += OnWindowClosed;
    }

    private void ToolBarOnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.Source is not AccessText) ToolBar.Close();
    }

    private void OnWindowClosed(object? o, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // _player.Dispose();
            // _libVlc.Dispose();
        });
        
        ToolBar.PointerPressed -= ToolBarOnPointerPressed;
        this.Closed -= OnWindowClosed;
    }

    private void ZoomIn_OnClick(object? sender, RoutedEventArgs e)
    {
        TimelineGrid.MinWidth += 100;
    }

    private void ZoomOut_OnClick(object? sender, RoutedEventArgs e)
    {
        TimelineGrid.MinWidth -= 100;
    }
}