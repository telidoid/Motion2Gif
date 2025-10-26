using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Motion2Gif.Player;
using Serilog;

namespace Motion2Gif.Processing;

public record MediaRange(TimeMs Start, TimeMs End)
{
    public static MediaRange Create(TimeMs start, TimeMs end)
    {
        if (end < start)
            throw new ArgumentException("end must be greater than start");

        return new MediaRange(start, end);
    }
}

public static class MediaRangeExtensions
{
    public static TimeMs GetDuration(this MediaRange mediaRange)
    {
        long ticks = mediaRange.End.TimeSpan().Ticks - mediaRange.Start.TimeSpan().Ticks;
        long ms = (ticks + 5_000) / 10_000; // округление к ближайшему мс
        return new TimeMs(ms);
    }
}

public static class VideoProcessor
{
    public static async Task CutVideo(MediaRange range, string input, string output, string ffmpegPath = "ffmpeg")
    {
        var start = range.Start.FormatForProcessor();
        var duration = range.GetDuration().FormatForProcessor();

        var args = $"-hide_banner -i \"{input}\" -ss {start} -t {duration} -c copy -map 0 -movflags +faststart \"{output}\"";

        Log.Information("ffmpeg args: " + args);

        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);

        if (process == null)
        {
            Log.Error("Could not start ffmpeg");
            return;
        }

        process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Log.Error("Ffmpeg exited with code " + process.ExitCode);
        }
    }
    
    public static async Task GenerateGif(
        MediaRange range,
        string input,
        string output,
        string ffmpegPath = "ffmpeg",
        int fps = 12,
        int maxWidth = 480,
        CancellationToken ct = default)
    {
        if (range is null) throw new ArgumentNullException(nameof(range));
        if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("input is empty", nameof(input));
        if (string.IsNullOrWhiteSpace(output)) throw new ArgumentException("output is empty", nameof(output));
        if (fps <= 0) throw new ArgumentOutOfRangeException(nameof(fps));
        if (maxWidth <= 0) throw new ArgumentOutOfRangeException(nameof(maxWidth));

        var start    = range.Start.FormatForProcessor();            // "HH:MM:SS.mmm"
        var duration = range.GetDuration().FormatForProcessor();    // "HH:MM:SS.mmm"

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

        psi.ArgumentList.Add("-y");                // перезаписывать выход
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
        psi.ArgumentList.Add($"[0:v]fps={fps},scale={maxWidth}:-1:flags=lanczos,split[v1][v2];[v1]palettegen=stats_mode=diff[p];[v2][p]paletteuse=dither=sierra2_4a");

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

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) Console.WriteLine(e.Data); };
        process.ErrorDataReceived  += (_, e) => { if (e.Data is not null) Console.WriteLine(e.Data); };

        
        if (!process.Start())
        {
            Log.Error("Could not start ffmpeg");
            return;
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var reg = ct.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                    _ = Task.Delay(500).ContinueWith(_ => { try { if (!process.HasExited) process.Kill(true); } catch { } });
                }
            }
            catch { }
        });

        await process.WaitForExitAsync(ct).ConfigureAwait(false);

        if (process.ExitCode != 0)
            Log.Error("ffmpeg (gif) exited with code {Code}", process.ExitCode);
    }
}