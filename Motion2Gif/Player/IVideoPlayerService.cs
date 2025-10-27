using System;
using System.Threading.Tasks;
using LibVLCSharp.Avalonia;

namespace Motion2Gif.Player;

public interface IVideoPlayerService
{
    void AttachPlayer(VideoView videoView);
    Task<VideoFileDescription> OpenAsync(Uri uri);
    void TogglePlay();
    void ToggleMute();
    void Stop();
    void ChangeTimePosition(long timePosition);
    void ChangeVolume(AudioVolume volume);
    Action<long> PlayerTimeChangedAction { get; set; }
    Action<PlayerState> PlayerStateChangedAction { get; set; }
    Action<bool> IsMutedStateChangedAction { get; set; }
}

public enum PlayerState
{
    Playing,
    Paused,
    Stopped,
}

public class DesignVideoPlayerService : IVideoPlayerService
{
    public void AttachPlayer(VideoView videoView) { }

    public Task<VideoFileDescription> OpenAsync(Uri uri)
    {
        throw new Exception("Design time video player service");
    }

    public void TogglePlay() { }

    public void ToggleMute() { }

    public void Stop() { }

    public void ChangeTimePosition(long timePosition) { }

    public void ChangeVolume(AudioVolume volume) { }

    public Action<long> PlayerTimeChangedAction { get; set; } = _ => { };
    public Action<PlayerState> PlayerStateChangedAction { get; set; } = _ => { };
    public Action<bool> IsMutedStateChangedAction { get; set; } = _ => { };
}