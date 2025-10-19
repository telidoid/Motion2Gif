using System;
using System.Threading.Tasks;
using Avalonia.Automation;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Motion2Gif.Controls;
using Serilog;

namespace Motion2Gif.Other;

public record VideoFileDescription(string Name, TimeMs Duration);

public static class DurationExtensions
{
    public static string Formatted(this TimeMs duration)
    {
        var ts = new TimeSpan(duration.Value);
        return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }
}

public interface IVideoPlayerService
{
    void AttachPlayer(VideoView videoView);
    Task<VideoFileDescription> OpenAsync(Uri uri);
    void Play();
    void Pause();
    void Stop();
    void ChangeTimePosition(long timePosition);
    void ChangeVolume(AudioVolume volume);
    Action<long> PlayerTimeChangedAction { get; set; }
}

public class VideoPlayerService : IVideoPlayerService, IDisposable
{
    private readonly MediaPlayer _player;
    private readonly LibVLC _libVlc = new();

    public VideoPlayerService()
    {
        _player = new MediaPlayer(_libVlc);
        _player.TimeChanged += (_, _) => this.PlayerTimeChangedAction(_player.Time);
        _player.EndReached += (_, _) => this.PlayerTimeChangedAction(_player.Media!.Duration);
    }

    public void AttachPlayer(VideoView videoView)
        => videoView.MediaPlayer = _player;
    
    public void Play() => _player.Play();

    public void Pause() => _player.Pause();
    
    public void Stop() => _player.Stop();

    public void ChangeVolume(AudioVolume volume) => _player.Volume = volume.Value;

    public Action<long> PlayerTimeChangedAction { get; set; } = _ => { };
    
    public async Task<VideoFileDescription> OpenAsync(Uri uri)
    {
        using var media = new Media(_libVlc, uri);
        _player.Play(media);

        await media.Parse();
        
        return new VideoFileDescription(uri.OriginalString, new TimeMs(media.Duration));
    }

    public void ChangeTimePosition(long timePosition)
    {
        _player.Time = timePosition;
    }

    public void Dispose()
    {
        _player.Dispose();
        _libVlc.Dispose();
    }
}