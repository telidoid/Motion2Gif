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
}