using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Motion2Gif.Processing;

public interface IJobProcessingService
{
    public JobId ScheduleJob(IJobModel jobModel);
    public event Action<Job, JobProgress>? OnStateChanged;
}

public class DesignJobProcessingService : IJobProcessingService
{
    public JobId ScheduleJob(IJobModel jobModel)
    {
        throw new Exception("Design time job processing service");
    }

    public event Action<Job, JobProgress>? OnStateChanged;
}

public class JobProcessingService : IJobProcessingService
{
    private readonly ConcurrentDictionary<JobId, Job> _jobs = new();
    private readonly SemaphoreSlim _semaphore = new(4, 4);
    public event Action<Job, JobProgress>? OnStateChanged;

    public JobId ScheduleJob(IJobModel jobModel)
    {
        var cts = new CancellationTokenSource();;
        var job = new Job(JobId.Create(), jobModel, cts);
        _jobs.TryAdd(job.Id, job);
        OnStateChanged?.Invoke(job, new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Queued));
        _ = ProcessJobAsync(job);

        return job.Id;
    }

    private async Task ProcessJobAsync(Job job)
    {
        await _semaphore.WaitAsync(job.CancellationTokenSource.Token);
        OnStateChanged?.Invoke(job, new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Running));

        try
        {
            await VideoProcessor.ProcessVideo(job.Model, progress => OnStateChanged?.Invoke(job, progress), job.CancellationTokenSource.Token);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}