using System;

namespace Motion2Gif.ViewModels;

// one window = one VM
public abstract partial class DialogViewModel : ViewModelBase
{
    public event EventHandler? OnCloseRequested = null;

    protected void Close()
    {
        this.OnCloseRequested!.Invoke(this, EventArgs.Empty);
    }
}