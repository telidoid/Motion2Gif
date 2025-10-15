using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public string MediaDuration { get; private set; } = "00:00:00";

    [ObservableProperty] private float _startingPosition;
    [ObservableProperty] private float _endingPosition;
    
    public MainWindowViewModel(IFilePickerService filePickerService)
    {
        FilePickerService = filePickerService;
        
        OpenVideoFileCmd = new AsyncRelayCommand(OnVideoFileOpened);
        PlayCmd = new RelayCommand(() => PlayerService.Play());
        PauseCmd = new RelayCommand(() => PlayerService.Pause());
        StopCmd = new RelayCommand(() => PlayerService.Stop());

        ChangePositionCmd = new RelayCommand(() => PlayerService.ChangePosition(_startingPosition));
    }

    private async Task OnVideoFileOpened()
    {
        var path = await FilePickerService.Pick();
        var fileDescription = await PlayerService.OpenAsync(path);
        MediaDuration = fileDescription.Duration.Formatted();
    }
}