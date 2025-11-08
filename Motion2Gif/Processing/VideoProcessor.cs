using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Motion2Gif.Player;
using Serilog;

namespace Motion2Gif.Processing;

public static class VideoProcessor
{
    public static async Task CutVideo(
        CutVideoJob jobModel,
        Action<JobProgress>? onStateChanged = null,
        CancellationToken ct = default,
        string ffmpegPath = "ffmpeg")
        => await CutVideo(
            jobModel.MediaRange,
            jobModel.FilePath.LocalPath,
            jobModel.OutputFilePath,
            onStateChanged: onStateChanged,
            ct: ct,
            ffmpegPath: ffmpegPath);

    private static async Task CutVideo(
        MediaRange range,
        string input,
        string output,
        Action<JobProgress>? onStateChanged = null,
        CancellationToken ct = default,
        string ffmpegPath = "ffmpeg")
    {
        var start = range.Start.FormatForProcessor();
        var duration = range.GetDuration().FormatForProcessor();
        var total = range.GetDuration().TimeSpan();

        var args =
            $"-hide_banner -ss {start} -t {duration} -i \"{input}\" -c copy -map 0 -movflags " +
            $"+faststart -progress pipe:1 \"{output}\"";

        var psi = new ProcessStartInfo  
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };

        var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        await using var reg = ct.Register(() =>
        {
            onStateChanged?.Invoke(new JobProgress(0, TimeSpan.Zero, total, JobState.Canceled));
            
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch
            {
                // ignored
            }
        });

        if (!process.Start())
        {
            Log.Error("Could not start ffmpeg.");
            onStateChanged?.Invoke(new JobProgress(0, TimeSpan.Zero, total, JobState.Failed));
            return;
        }

        _ = Task.Run(async () =>
        {
            while (await process.StandardError.ReadLineAsync(ct) is
               {
                   /* ignored */
               } _) ;
        }, ct);

        var processed = TimeSpan.Zero;

        while (!process.HasExited && await process.StandardOutput.ReadLineAsync(ct) is { } line)
        {
            var keyVal = line.Split('=');
            var (key, value) = (keyVal[0], keyVal[1]);

            if (key == "out_time_ms" && long.TryParse(value, out var outTimeMs))
            {
                processed = TimeSpan.FromMicroseconds(outTimeMs);
                var percent = total.TotalMilliseconds > 0
                    ? Math.Clamp(processed.TotalMilliseconds * 100.0 / total.TotalMilliseconds, 0, 100)
                    : 100.0;

                onStateChanged?.Invoke(new JobProgress(percent, processed, total, JobState.Running));
            }
            else if (key == "progress")
            {
                var percent = value == "end" ? 100.0
                    : total.TotalMilliseconds > 0 ? Math.Clamp(processed.TotalMilliseconds * 100.0 / total.TotalMilliseconds, 0, 100)
                    : 100.0;
                
                if (percent >= 100)
                    onStateChanged?.Invoke(new JobProgress(percent, processed, total, JobState.Completed));
            }
        }

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0 && !ct.IsCancellationRequested)
        {
            Log.Error("ffmpeg exited with code {Code}", process.ExitCode);
            onStateChanged?.Invoke(new JobProgress(0, processed, total, JobState.Failed));
        }
    }

    public static async Task GenerateGif(
        GenerateGifJob jobModel,
        Action<JobProgress>? onStateChanged = null,
        CancellationToken ct = default,
        string ffmpegPath = "ffmpeg")
        => await GenerateGif(
            jobModel.MediaRange,
            jobModel.FilePath.LocalPath,
            jobModel.OutputFilePath,
            onStateChanged: onStateChanged,
            ct: ct,
            ffmpegPath: ffmpegPath);

    private static async Task GenerateGif(
        MediaRange range,
        string input,
        string output,
        string ffmpegPath = "ffmpeg",
        int fps = 12,
        int maxWidth = 480,
        Action<JobProgress>? onStateChanged = null,
        CancellationToken ct = default)
    {
        if (range is null) throw new ArgumentNullException(nameof(range));
        if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("input is empty", nameof(input));
        if (string.IsNullOrWhiteSpace(output)) throw new ArgumentException("output is empty", nameof(output));
        if (fps <= 0) throw new ArgumentOutOfRangeException(nameof(fps));
        if (maxWidth <= 0) throw new ArgumentOutOfRangeException(nameof(maxWidth));

        var start = range.Start.FormatForProcessor(); // "HH:MM:SS.mmm"
        var duration = range.GetDuration().FormatForProcessor(); // "HH:MM:SS.mmm"

        // Один проход: сплитим поток → генерим палитру → применяем палитру
        // - точный seek: -i ... -ss ... -t ...
        // - отключаем аудио: -an
        // - scale до maxWidth, сохраняем соотношение: высота -1
        // - качественный ресемплинг: lanczos
        // - качественное дизеринг: sierra2_4a
        // - бесконечный цикл: -loop 0
        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        psi.ArgumentList.Add("-y"); // перезаписывать выход
        psi.ArgumentList.Add("-hide_banner");

        // вход
        psi.ArgumentList.Add("-i");
        psi.ArgumentList.Add(input);

        // точная нарезка
        psi.ArgumentList.Add("-ss");
        psi.ArgumentList.Add(start);
        psi.ArgumentList.Add("-t");
        psi.ArgumentList.Add(duration);

        // фильтр для GIF
        // Пример: [0:v]fps=12,scale=480:-1:flags=lanczos,split[v1][v2];[v1]palettegen=stats_mode=diff[p];[v2][p]paletteuse=dither=sierra2_4a
        psi.ArgumentList.Add("-filter_complex");
        psi.ArgumentList.Add(
            $"[0:v]fps={fps},scale={maxWidth}:-1:flags=lanczos,split[v1][v2];[v1]palettegen=stats_mode=diff[p];[v2][p]paletteuse=dither=sierra2_4a");

        // без аудио
        psi.ArgumentList.Add("-an");

        // флаги GIF: -loop 0 = бесконечное повторение
        psi.ArgumentList.Add("-gifflags");
        psi.ArgumentList.Add("-offsetting");
        psi.ArgumentList.Add("-loop");
        psi.ArgumentList.Add("0");

        // выход
        psi.ArgumentList.Add(output);

        Log.Information("ffmpeg (gif) args: {Args}", string.Join(' ', psi.ArgumentList));

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null) Console.WriteLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null) Console.WriteLine(e.Data);
        };
        
        if (!process.Start())
        {
            Log.Error("Could not start ffmpeg");
            return;
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await using var reg = ct.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                    _ = Task.Delay(500).ContinueWith(_ =>
                    {
                        try
                        {
                            if (!process.HasExited) process.Kill(true);
                        }
                        catch
                        {
                        }
                    });
                }
            }
            catch
            {
            }
        });

        await process.WaitForExitAsync(ct).ConfigureAwait(false);

        if (process.ExitCode != 0)
            Log.Error("ffmpeg (gif) exited with code {Code}", process.ExitCode);
    }
}