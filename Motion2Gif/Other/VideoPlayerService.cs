using System;
using System.Threading.Tasks;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;

namespace Motion2Gif.Other;

public record VideoFileDescription(string Name, MediaDuration Duration);
public record MediaDuration(long DurationInMs);

public static class DurationExtensions
{
    public static string Formatted(this MediaDuration duration)
    {
        var ts = new TimeSpan(duration.DurationInMs);
        return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }
}

public interface IVideoPlayerService
{
    void AttachPlayer(VideoView videoView);
    void Play();
    void Pause();
    void Stop();
    Task<VideoFileDescription> OpenAsync(Uri uri);
}

public class VideoPlayerService : IVideoPlayerService, IDisposable
{
    private readonly MediaPlayer _player;
    private readonly LibVLC _libVlc = new();

    public VideoPlayerService()
    {
        _player = new MediaPlayer(_libVlc);
    }

    public void AttachPlayer(VideoView videoView)
        => videoView.MediaPlayer = _player;
    
    public void Play() => _player.Play();

    public void Pause() => _player.Pause();
    
    public void Stop() => _player.Stop();
    
    public async Task<VideoFileDescription> OpenAsync(Uri uri)
    {
        using var media = new Media(_libVlc, uri);
        _player.Play(media);

        await media.Parse();
        
        return new VideoFileDescription(uri.OriginalString, new MediaDuration(media.Duration));
    }

    public void Dispose()
    {
        _player.Dispose();
        _libVlc.Dispose();
    }
}