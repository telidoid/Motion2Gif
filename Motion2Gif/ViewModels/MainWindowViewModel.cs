using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Motion2Gif.Other;

namespace Motion2Gif.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public IVideoPlayerService PlayerService = new VideoPlayerService();
    public IFilePickerService FilePickerService;

    public ICommand PlayCmd { get; }
    public ICommand PauseCmd { get; }
    public ICommand StopCmd { get; }
    public ICommand OpenVideoFile { get; }

    public string MediaDuration { get; private set; } = "00:00:00";
    
    public MainWindowViewModel(IFilePickerService filePickerService)
    {
        FilePickerService = filePickerService;
        
        PlayCmd = new RelayCommand(() => PlayerService.Play());
        PauseCmd = new RelayCommand(() => PlayerService.Pause());
        StopCmd = new RelayCommand(() => PlayerService.Stop());
        
        OpenVideoFile = new AsyncRelayCommand(OnVideoFileOpened);
    }

    private async Task OnVideoFileOpened()
    {
        var path = await FilePickerService.Pick();
        var fileDescription = await PlayerService.OpenAsync(path);
        MediaDuration = fileDescription.Duration.Formatted();
    }
}