using System;
using System.Threading.Tasks;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Motion2Gif.Player;

namespace Motion2Gif.VLC;

public class VideoPlayerService : IVideoPlayerService, IDisposable
{
    private readonly MediaPlayer _player;
    private readonly LibVLC _libVlc = new();
    private long _userDefinedTimePosition = 0;

    public VideoPlayerService()
    {
        _player = new MediaPlayer(_libVlc);
        _player.Mute = false;
        _player.Stopped += (_, _) => PlayerStateChangedAction(PlayerState.Stopped);
        _player.Playing += (_, _) => PlayerStateChangedAction(PlayerState.Playing);
        _player.Paused += (_, _) => PlayerStateChangedAction(PlayerState.Paused);
        _player.TimeChanged += (_, _) => this.PlayerTimeChangedAction(_player.Time);
        _player.Muted += (_, _) => this.IsMutedStateChangedAction(true);
        _player.Unmuted += (_, _) => this.IsMutedStateChangedAction(false);
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

    public void ToggleMute() => _player.ToggleMute();

    public void Stop()
    {
        if (_player.State is VLCState.Stopped)
            return;

        _player.Stop();
    }

    public void ChangeVolume(AudioVolume volume) => _player.Volume = volume.Value;

    public Action<long> PlayerTimeChangedAction { get; set; } = _ => { };
    public Action<PlayerState> PlayerStateChangedAction { get; set; } = _ => { };
    public Action<bool> IsMutedStateChangedAction { get; set; } = _ => { };

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
                _player.PositionChanged -= OnPositionChanged;
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