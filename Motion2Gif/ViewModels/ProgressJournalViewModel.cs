using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Motion2Gif.Processing;
using Serilog;

namespace Motion2Gif.ViewModels;

public partial class ProgressJournalViewModel : ViewModelBase
{
    private readonly IJobProcessingService _jobProcessingService;

    public ObservableCollection<JobViewModel> Jobs { get; } = new();

    [ObservableProperty] private bool _isJobListEmpty = true;
    [ObservableProperty] private string _topText = "";

    public ProgressJournalViewModel(IJobProcessingService jobProcessingService)
    {
        _jobProcessingService = jobProcessingService;

        _jobProcessingService.OnStateChanged += OnJobStateChanged;
    }

    private void OnJobStateChanged(Job job, JobProgress progress)
        => Dispatcher.UIThread.Post(() => ApplyUpdate(job, progress));

    private void ApplyUpdate(Job job, JobProgress progress)
    {
        if (progress.State == JobState.Queued && this.Jobs.All(x => x.JobId != job.Id))
        {
            Jobs.Add(new JobViewModel
            {
                JobId = job.Id,
                CancellationTokenSource = job.CancellationTokenSource,
                Percentage = progress.Percent,
                State = progress.State,
                RemoveCmd = new RelayCommand<JobViewModel>(vm =>
                {
                    if (vm != null)
                    {
                        vm.CancellationTokenSource.Cancel();
                        Jobs.Remove(vm);
                    }

                    Refresh();
                }),
                CancelCmd = new RelayCommand<JobViewModel>(vm =>
                {
                    vm?.CancellationTokenSource.Cancel();
                    Refresh();
                })
            });
        }
        else
        {
            var updatedJob = Jobs.Single(x => x.JobId == job.Id);
            updatedJob.Percentage = progress.Percent;
            updatedJob.State = progress.State;
        }

        Refresh();
    }

    private void Refresh()
    {
        IsJobListEmpty = !Jobs.Any();
        
        var qtyOnProcess = Jobs.Count(x => x.State is JobState.Queued or JobState.Running);
        
        foreach (var job in Jobs)
            Log.Information(job.State.ToString());
        
        TopText = qtyOnProcess != 0 ? $"Processing ({qtyOnProcess})" : $"All ({Jobs.Count})";
    }
    
    public void Dispose()
    {
        _jobProcessingService.OnStateChanged -= OnJobStateChanged;
    }
}

public partial class JobViewModel : ObservableObject
{
    public required JobId JobId { get; set; }
    public required CancellationTokenSource CancellationTokenSource { get; set; }
    [ObservableProperty] private double _percentage;
    [ObservableProperty] private JobState _state;
    public required IRelayCommand<JobViewModel> RemoveCmd { get; set; }
    public required IRelayCommand<JobViewModel> CancelCmd { get; set; }
}