using Motion2Gif.Other;
using Motion2Gif.Player;
using Motion2Gif.Processing;

namespace Motion2Gif.ViewModels;

public class DesignMainWindowViewModel(
    IVideoPlayerService playerService,
    IFilePickerService filePickerService,
    IJobProcessingService jobProcessingService,
    IDialogService dialogService)
    : MainWindowViewModel(playerService, filePickerService, jobProcessingService, dialogService)
{
    public DesignMainWindowViewModel() 
        : this(
            new DesignVideoPlayerService(), 
            new DesignFilePickerService(), 
            new DesignJobProcessingService(),
            new DesignDialogService()
            )
    {
    }
}