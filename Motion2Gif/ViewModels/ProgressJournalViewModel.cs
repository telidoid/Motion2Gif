using Motion2Gif.Processing;

namespace Motion2Gif.ViewModels;

public class ProgressJournalViewModel : ViewModelBase
{
    private readonly IJobProcessingService _jobProcessingService;

    public ProgressJournalViewModel(IJobProcessingService jobProcessingService)
    {
        _jobProcessingService = jobProcessingService;
    }
    
    
}