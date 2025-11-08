using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Motion2Gif.Processing;

public interface IJobProcessingService
{
    public JobId ScheduleJob(IJobModel jobModel, CancellationToken ct = default);
    public event Action<Job, JobProgress>? OnStateChanged;
}

public class DesignJobProcessingService : IJobProcessingService
{
    public JobId ScheduleJob(IJobModel jobModel, CancellationToken ct = default)
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

    public JobId ScheduleJob(IJobModel jobModel, CancellationToken ct = default)
    {
        var job = new Job(JobId.Create(), jobModel, ct);
        _jobs.TryAdd(job.Id, job);
        OnStateChanged?.Invoke(job, new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Queued));
        _ = ProcessJobAsync(job);

        return job.Id;
    }

    private async Task ProcessJobAsync(Job job)
    {
        await _semaphore.WaitAsync(job.CancellationToken);
        OnStateChanged?.Invoke(job, new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Running));

        try
        {
            switch (job.Model)
            {
                case CutVideoJob cutVideoJobModel:
                    await VideoProcessor.CutVideo(
                        cutVideoJobModel,
                        p => OnStateChanged?.Invoke(job, p),
                        ct: job.CancellationToken);
                    break;

                case GenerateGifJob generateGifJobModel:
                    await VideoProcessor.GenerateGif(
                        generateGifJobModel,
                        p => OnStateChanged?.Invoke(job, p),
                        ct: job.CancellationToken);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            OnStateChanged?.Invoke(job, new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Failed));
            Log.Error("Error processing job", ex);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}