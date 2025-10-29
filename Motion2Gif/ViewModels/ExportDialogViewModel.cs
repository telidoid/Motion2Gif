using CommunityToolkit.Mvvm.Input;

namespace Motion2Gif.ViewModels;

public partial class ExportDialogViewModel: DialogViewModel
{
    [RelayCommand]
    private void Submit()
    {
        this.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        this.Close();
    }
}