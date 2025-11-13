using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using Motion2Gif.Player;
using Serilog;

namespace Motion2Gif.Processing;

public static class VideoProcessor
{
    public static async Task ProcessVideo(
        IJobModel jobModel,
        Action<JobProgress>? onStateChanged,
        CancellationToken ctx
    )
    {
        var command = jobModel switch
        {
            CutVideoJob j => CutVideo(j, onStateChanged, ctx),
            GenerateGifJob j => GenerateGif(j, onStateChanged, ctx),
            _ => throw new ArgumentOutOfRangeException(nameof(jobModel), jobModel, null)
        };

        try
        {
            await command.ExecuteAsync(ctx);
        }
        catch (OperationCanceledException ex)
        {
            onStateChanged?.Invoke(new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Canceled));
            File.Delete(jobModel.OutputFilePath);
        }
        catch (Exception ex)
        {
            onStateChanged?.Invoke(new JobProgress(0, TimeSpan.Zero, TimeSpan.Zero, JobState.Failed));
            Log.Error("Error processing job", ex);
        }
    }

    private static Command CutVideo(CutVideoJob job, Action<JobProgress>? onStateChanged, CancellationToken ctx)
    {
        var start = job.MediaRange.Start.FormatForProcessor();
        var duration = job.MediaRange.GetDuration().FormatForProcessor();
        var total = job.MediaRange.GetDuration().TimeSpan();

        var input = job.FilePath.LocalPath;
        var output = job.OutputFilePath;

        return Cli.Wrap("ffmpeg")
            .WithArguments(args => args
                .Add("-hide_banner")
                .Add("-ss").Add(start)
                .Add("-t").Add(duration)
                .Add("-i").Add(input)
                .Add("-c").Add("copy")
                .Add("-map").Add("0")
                .Add("-movflags").Add("+faststart")
                .Add("-progress").Add("pipe:1")
                .Add(output)
            ) | (PipeTarget.ToDelegate(l => StdOut(l, total, onStateChanged, ctx)), PipeTarget.ToDelegate(StdErr));
    }

    private static Command GenerateGif(GenerateGifJob job, Action<JobProgress>? onStateChanged, CancellationToken ctx)
    {
        var start = job.MediaRange.Start.FormatForProcessor();
        var duration = job.MediaRange.GetDuration().FormatForProcessor();
        var total = job.MediaRange.GetDuration().TimeSpan();

        var input = job.FilePath.LocalPath;
        var output = job.OutputFilePath;

        var fps = 10;
        var maxWidth = 320;

        return Cli.Wrap("ffmpeg")
            .WithArguments(args => args
                .Add("-y")
                .Add("-hide_banner")
                .Add("-ss").Add(start)
                .Add("-t").Add(duration)
                .Add("-i").Add(input)
                // .Add("-ss").Add("0") // точная подправка на выходе, сдвиг от уже подрезанного старта (обычно 0)
                // .Add("-t").Add(duration)
                .Add("-filter_complex").Add($"[0:v]fps={fps},scale={maxWidth}:-1:flags=lanczos,split[v1][v2];[v1]palettegen=stats_mode=diff[p];[v2][p]paletteuse=dither=sierra2_4a")
                .Add("-an")
                .Add("-gifflags").Add("-offsetting")
                .Add("-loop").Add("0")
                .Add("-progress").Add("pipe:1")
                .Add(output)
            ) | (PipeTarget.ToDelegate(l => StdOut(l, total, onStateChanged, ctx)), PipeTarget.ToDelegate(StdErr));
    }

    private static void StdOut(string line, TimeSpan total, Action<JobProgress>? onStateChanged, CancellationToken ctx)
    {
        Log.Information($"{line}, ctx: {ctx.IsCancellationRequested}");
        
        var keyVal = line.Split('=');
        var (key, value) = (keyVal[0], keyVal[1]);

        if (key == "out_time_ms" && long.TryParse(value, out var outTimeMs))
        {
            var processed = TimeSpan.FromMicroseconds(outTimeMs);
            var percent = total.TotalMilliseconds > 0
                ? Math.Clamp(processed.TotalMilliseconds * 100.0 / total.TotalMilliseconds, 0, 100)
                : 100.0;

            onStateChanged?.Invoke(new JobProgress(percent, processed, total, JobState.Running));
        }
        
        if (key == "progress" && value == "end")
            onStateChanged?.Invoke(new JobProgress(100, total, total, JobState.Completed));
    }

    private static void StdErr(string err)
    {
        Log.Error(err);
    }
}