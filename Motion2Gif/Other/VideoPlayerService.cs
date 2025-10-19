using System;
using System.Threading.Tasks;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Motion2Gif.Controls;
using Serilog;

namespace Motion2Gif.Other;

public record VideoFileDescription(string Name, TimeMs Duration);

public interface IVideoPlayerService
{
    void AttachPlayer(VideoView videoView);
    Task<VideoFileDescription> OpenAsync(Uri uri);
    void TogglePlay();
    void Stop();
    void ChangeTimePosition(long timePosition);
    void ChangeVolume(AudioVolume volume);
    Action<long> PlayerTimeChangedAction { get; set; }
}

public class VideoPlayerService : IVideoPlayerService, IDisposable
{
    private readonly MediaPlayer _player;
    private readonly LibVLC _libVlc = new();
    private long _userDefinedTimePosition = 0;

    public VideoPlayerService()
    {
        _player = new MediaPlayer(_libVlc);
        _player.TimeChanged += (_, _) => this.PlayerTimeChangedAction(_player.Time);
        _player.EndReached += (_, _) =>
        {
            this.PlayerTimeChangedAction(_player.Media!.Duration);
            _userDefinedTimePosition = 0;
        };
    }

    public void AttachPlayer(VideoView videoView)
        => videoView.MediaPlayer = _player;
    
    public void TogglePlay()
    {
        switch (_player.State)
        {
            case VLCState.Stopped:
                _player.Play();
                _player.Time = _userDefinedTimePosition;
                break;
            case VLCState.Ended:
                _player.Stop();
                _player.Play();
                _player.Time = _userDefinedTimePosition;
                break;
            default:
                _player.Pause();
                break;
        }
    }

    public void Stop()
    {
        if (_player.State is VLCState.Stopped)
            return;
        
        _player.Stop();
    }

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
        _userDefinedTimePosition = timePosition;

        if (_player.State is VLCState.Ended)
        {
            _player.Stop();
            
            void OnPositionChanged(object? o, EventArgs eventArgs)
            {
                _player.Pause();
                _player.PositionChanged -=  OnPositionChanged;
            }

            _player.PositionChanged += OnPositionChanged;
            
            _player.Play();
            _player.Time = _userDefinedTimePosition;
        }
    }

    public void Dispose()
    {
        _player.Dispose();
        _libVlc.Dispose();
    }
}