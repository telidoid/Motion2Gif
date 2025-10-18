using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Motion2Gif.Other;
using Motion2Gif.ViewModels;
using Serilog;

namespace Motion2Gif.Views;

public partial class MainWindow : Window
{
    private static readonly Resolution MinResolution = new(400, 400);
    
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
        
        this.MinHeight = MinResolution.Height;
        this.MinWidth = MinResolution.Width;
        VideoView.MinHeight = MinResolution.Height;
        VideoView.MinWidth = MinResolution.Width;
        
        this.Closed += OnWindowClosed;
    }

    private void OnWindowClosed(object? o, EventArgs e)
    {
        Log.Information("Main windows is closed");
        
        Dispatcher.UIThread.Post(() =>
        {
            // _player.Dispose();
            // _libVlc.Dispose();
        });
    }
}