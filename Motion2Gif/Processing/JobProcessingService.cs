using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Motion2Gif.Processing;

public interface IJobProcessingService
{
    public JobId ScheduleJob(IJobModel jobModel, IProgress<JobProgress>? progress = null, CancellationToken ct = default);
}

public class DesignJobProcessingService : IJobProcessingService
{
    public JobId ScheduleJob(IJobModel jobModel, IProgress<JobProgress>? progress = null, CancellationToken ct = default)
    {
        throw new Exception("Design time job processing service");
    }
}

public class JobProcessingService: IJobProcessingService
{
    private readonly ConcurrentDictionary<JobId, Job> _jobs = new();
    private readonly SemaphoreSlim _semaphore = new(4, 4);

    public JobId ScheduleJob(IJobModel jobModel, IProgress<JobProgress>? progress = null,
        CancellationToken ct = default)
    {
        var job = new Job(JobId.Create(), progress, jobModel, ct);
        _jobs.TryAdd(job.Id, job);
        _ = ProcessJobAsync(job);
        progress?.Report(new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Queued, "Queued"));

        return job.Id;
    }

    private async Task ProcessJobAsync(Job job)
    {
        await _semaphore.WaitAsync(job.CancellationToken);

        job.Progress?.Report(new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Running, "Running"));

        try
        {
            switch (job.Model)
            {
                case CutVideoJob cutVideoJobModel:
                    await VideoProcessor.CutVideo(cutVideoJobModel, progress: job.Progress, ct: job.CancellationToken);
                    break;
                case GenerateGifJob generateGifJobModel:
                    await VideoProcessor.GenerateGif(generateGifJobModel, progress: job.Progress,
                        ct: job.CancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            job.Progress?.Report(new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Failed, "Failed"));
            Log.Error("Error processing job", ex);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}