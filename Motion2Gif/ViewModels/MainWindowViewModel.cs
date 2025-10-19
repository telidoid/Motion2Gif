using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Motion2Gif.Controls;
using Motion2Gif.Other;
// using static Motion2Gif.Other.AudioVolume;

namespace Motion2Gif.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public readonly IVideoPlayerService PlayerService = new VideoPlayerService();
    private readonly IFilePickerService _filePickerService;

    public ICommand OpenVideoFileCmd { get; }
    public ICommand TogglePlayCmd { get; }
    public ICommand StopCmd { get; }

    [ObservableProperty] private TimeMs _currentPosition = new(0);
    [ObservableProperty] private TimeMs _mediaDuration = new(0);
    [ObservableProperty] private int _volume = 100;
    
    private bool _suppressPlayerSeek;
    
    public MainWindowViewModel(IFilePickerService filePickerService)
    {
        _filePickerService = filePickerService;
        
        OpenVideoFileCmd = new AsyncRelayCommand(OnVideoFileOpened);
        TogglePlayCmd = new RelayCommand(() => PlayerService.TogglePlay());
        StopCmd = new RelayCommand(() =>
        {
            CurrentPosition = new TimeMs(0);
            PlayerService.Stop();
        });

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
        
        PlayerService.ChangeVolume(AudioVolume.Create(Volume));
    }

    private async Task OnVideoFileOpened()
    {
        var path = await _filePickerService.Pick();
        
        if (path == null)
            return;
        
        var fileDescription = await PlayerService.OpenAsync(path);
        MediaDuration = fileDescription.Duration;
        CurrentPosition = new TimeMs(0);
    }

    partial void OnCurrentPositionChanged(TimeMs value)
    {
        if (_suppressPlayerSeek) return;
        PlayerService.ChangeTimePosition(value.Value);
    }

    partial void OnVolumeChanged(int value) => 
        PlayerService.ChangeVolume(AudioVolume.Create(value));
}