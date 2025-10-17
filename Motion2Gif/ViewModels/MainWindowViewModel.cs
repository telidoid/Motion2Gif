using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Motion2Gif.Controls;
using Motion2Gif.Other;

namespace Motion2Gif.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public readonly IVideoPlayerService PlayerService = new VideoPlayerService();
    public readonly IFilePickerService FilePickerService;

    public ICommand OpenVideoFileCmd { get; }
    public ICommand PlayCmd { get; }
    public ICommand PauseCmd { get; }
    public ICommand StopCmd { get; }
    public ICommand ChangePositionCmd { get; }

    [ObservableProperty] private TimeMs _currentPosition = new(0);
    [ObservableProperty] private TimeMs _mediaDuration = new(0);
    
    private bool _suppressPlayerSeek;
    
    public MainWindowViewModel(IFilePickerService filePickerService)
    {
        FilePickerService = filePickerService;
        
        OpenVideoFileCmd = new AsyncRelayCommand(OnVideoFileOpened);
        PlayCmd = new RelayCommand(() => PlayerService.Play());
        PauseCmd = new RelayCommand(() => PlayerService.Pause());
        StopCmd = new RelayCommand(() => PlayerService.Stop());

        ChangePositionCmd = new RelayCommand(() => PlayerService.ChangeTimePosition(0));
        
        PlayerService.PlayerTimeChangedAction = l =>
        {
            try
            {
                _suppressPlayerSeek = true;
                CurrentPosition = new TimeMs(l);
            }
            finally
            {
                _suppressPlayerSeek = false;
            }
        };
    }

    private async Task OnVideoFileOpened()
    {
        var path = await FilePickerService.Pick();
        var fileDescription = await PlayerService.OpenAsync(path);
        MediaDuration = fileDescription.Duration;
        CurrentPosition = new TimeMs(0);
    }

    partial void OnCurrentPositionChanged(TimeMs value)
    {
        if (_suppressPlayerSeek) return;
        PlayerService.ChangeTimePosition(value.Value);
    }
}