using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Motion2Gif.ViewModels;

public partial class ExportDialogViewModel: DialogViewModel
{
    [ObservableProperty] private GenGifConfigViewModel _gifConfigVm = new();
    [ObservableProperty] private VideoCutConfigViewModel _cutConfigVm = new();
    [ObservableProperty] private IProcessConfigViewModel? _selected;

    [ObservableProperty] private bool _isCutChosen;
    [ObservableProperty] private bool _isGifChosen;

    public IProcessConfigViewModel? Result { get; set; }

    public ExportDialogViewModel()
    {
        Selected = _cutConfigVm;
        IsCutChosen = true;
    }

    [RelayCommand]
    private void Submit()
    {
        this.Result = Selected;
        this.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        this.Result = null;
        this.Close();
    }

    [RelayCommand]
    private void GifChosen()
    {
        Selected = GifConfigVm;
        IsCutChosen = false;
        IsGifChosen = true;
    }

    [RelayCommand]
    private void CutChosen()
    {
        Selected = CutConfigVm;
        IsCutChosen = true;
        IsGifChosen = false;
    }
}