using System;
using System.Collections;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Motion2Gif.ViewModels;

public interface IProcessConfigViewModel: INotifyDataErrorInfo
{
}

public class GenGifConfigViewModel : ObservableObject, IProcessConfigViewModel
{
    public IEnumerable GetErrors(string? propertyName)
    {
        throw new NotImplementedException();
    }

    public bool HasErrors { get; }
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
}

public class VideoCutConfigViewModel : ObservableObject, IProcessConfigViewModel
{
    public IEnumerable GetErrors(string? propertyName)
    {
        throw new NotImplementedException();
    }

    public bool HasErrors { get; }
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
}