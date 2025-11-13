using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Avalonia;
using Motion2Gif.Other;
using Motion2Gif.Player;
using Motion2Gif.Processing;
using Serilog;

namespace Motion2Gif.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IVideoPlayerService _playerService;
    private readonly IFilePickerService _filePickerService;
    private readonly IJobProcessingService _jobProcessingService;
    private readonly IDialogService _dialogService;

    public ICommand OpenVideoFileCmd { get; }
    public ICommand ToggleMuteCmd { get; }
    public ICommand TogglePlayCmd { get; }
    public ICommand StopCmd { get; }
    public ICommand ExportCmd { get; }

    [ObservableProperty] private TimeMs _currentPosition = new(0);
    [ObservableProperty] private TimeMs _mediaDuration = new(0);
    [ObservableProperty] private int _volume = 100;
    [ObservableProperty] private string _displayedTime = "00:00 / 00:00";

    [ObservableProperty] private TimeMs _trimStart = new(0);
    [ObservableProperty] private TimeMs _trimEnd = new(2_000);
    [ObservableProperty] private TimeMs _selectorMin = new(0);

    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isMuted;

    private bool _suppressPlayerSeek;

    private VideoFileDescription? _openedFileDescription = null;

    public MainWindowViewModel(
        IVideoPlayerService playerService,
        IFilePickerService filePickerService,
        IJobProcessingService jobProcessingService,
        IDialogService dialogService)
    {
        _playerService = playerService;
        _filePickerService = filePickerService;
        _jobProcessingService = jobProcessingService;
        _dialogService = dialogService;

        OpenVideoFileCmd = new AsyncRelayCommand(OnVideoFileOpened);
        ToggleMuteCmd = new RelayCommand(() => _playerService.ToggleMute());
        TogglePlayCmd = new RelayCommand(() => _playerService.TogglePlay());
        StopCmd = new RelayCommand(StopVideo);
        ExportCmd = new AsyncRelayCommand(async () =>
        {
            var vm = await _dialogService.ShowDialog<ExportDialogViewModel>();
            var range = MediaRange.Create(TrimStart, TrimEnd);

            if (_openedFileDescription == null)
                return;

            if (vm.Result is VideoCutConfigViewModel cutConfig)
            {
                var outputPath = $"{Directory.GetCurrentDirectory()}_{Guid.NewGuid()}.mkv";
                _jobProcessingService.ScheduleJob(new CutVideoJob(range, _openedFileDescription.Uri, outputPath));
            }

            if (vm.Result is GenGifConfigViewModel gifConfig)
            {
                var outputPath = $"{Directory.GetCurrentDirectory()}_{Guid.NewGuid()}.gif";
                _jobProcessingService.ScheduleJob(new GenerateGifJob(range, _openedFileDescription.Uri, outputPath));
            }
        });

        _playerService.PlayerTimeChangedAction = l =>
        {
            try
            {
                _suppressPlayerSeek = true;
                CurrentPosition = new TimeMs(l);
                UpdateDisplayedTime();
            }
            finally
            {
                _suppressPlayerSeek = false;
            }
        };

        _playerService.PlayerStateChangedAction = state =>
        {
            IsPlaying = state switch
            {
                PlayerState.Playing => true,
                PlayerState.Paused => false,
                PlayerState.Stopped => false,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        };

        _playerService.IsMutedStateChangedAction = isMuted => IsMuted = isMuted;

        _playerService.ChangeVolume(AudioVolume.Create(Volume));
    }

    private void StopVideo()
    {
        CurrentPosition = new TimeMs(0);
        _playerService.Stop();
    }

    private async Task OnVideoFileOpened()
    {
        var path = await _filePickerService.Pick();

        if (path == null)
            return;

        _openedFileDescription = await _playerService.OpenAsync(path);
        MediaDuration = _openedFileDescription.Duration;
        CurrentPosition = new TimeMs(0);
    }

    public async Task OpenVideoFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        var fileDescription = await _playerService.OpenAsync(new Uri(path));
        MediaDuration = fileDescription.Duration;
        CurrentPosition = new TimeMs(0);
    }

    partial void OnCurrentPositionChanged(TimeMs value)
    {
        if (_suppressPlayerSeek)
            return;

        _playerService.ChangeTimePosition(value.Value);
        UpdateDisplayedTime();
    }

    private void UpdateDisplayedTime() =>
        DisplayedTime = $"{CurrentPosition.Formatted()} / {MediaDuration.Formatted()}";

    partial void OnVolumeChanged(int value) => _playerService.ChangeVolume(AudioVolume.Create(value));

    public void AttachPlayer(VideoView videoView) => _playerService.AttachPlayer(videoView);
}