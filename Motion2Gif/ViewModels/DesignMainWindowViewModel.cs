using Motion2Gif.Other;
using Motion2Gif.Player;
using Motion2Gif.Processing;

namespace Motion2Gif.ViewModels;

public class DesignMainWindowViewModel(
    IVideoPlayerService playerService,
    IFilePickerService filePickerService,
    IJobProcessingService jobProcessingService)
    : MainWindowViewModel(playerService, filePickerService, jobProcessingService)
{
    public DesignMainWindowViewModel() : this(new DesignVideoPlayerService(), new DesignFilePickerService(), new DesignJobProcessingService())
    {
    }
}