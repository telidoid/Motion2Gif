using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Motion2Gif.Processing;

public enum JobState
{
    Queued,
    Running,
    Completed,
    Canceled,
    Failed
}

public readonly record struct JobId(Guid Value)
{
    public static JobId Create() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public record JobProgress(double Percent, TimeSpan Processed, TimeSpan Total, JobState State, string? Phase);

public record Job(JobId Id, IProgress<JobProgress>? Progress, IJobModel Model, CancellationToken CancellationToken = default);

public interface IJobModel;

public record GenerateGifJob(MediaRange MediaRange, Uri FilePath, string OutputFilePath) : IJobModel;
public record CutVideoJob(MediaRange MediaRange, Uri FilePath, string OutputFilePath) : IJobModel;

public class JobProcessingService
{
    private readonly ConcurrentDictionary<JobId, Job> _jobs = new();
    private readonly SemaphoreSlim _semaphore = new(4, 4);

    public JobId ScheduleJob(IJobModel jobModel, IProgress<JobProgress>? progress = null, CancellationToken ct = default)
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
        try
        {
            switch (job.Model)
            {
                case CutVideoJob cutVideoJobModel:
                    await VideoProcessor.CutVideo(cutVideoJobModel, ct: job.CancellationToken);
                    break;
                case GenerateGifJob generateGifJobModel:
                    await VideoProcessor.GenerateGif(generateGifJobModel, ct: job.CancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error processing job", ex);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}