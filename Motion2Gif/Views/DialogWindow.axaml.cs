using System;
using Avalonia.Controls;
using Avalonia.Threading;
using Motion2Gif.ViewModels;

namespace Motion2Gif.Views;

public partial class DialogWindow : Window
{
    public DialogWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        Deactivated += OnDeactivated;

        if (DataContext is DialogViewModel vm)
            vm.OnCloseRequested += CloseRequestedFromVm;
    }

    // When user presses on vlc:VideoView dialog, it loses it's focus, that is a workaround:
    private void OnDeactivated(object? sender, EventArgs e) => 
        Dispatcher.UIThread.Post(Activate);
    
    protected override void OnClosed(EventArgs e)
    {
        Deactivated -= OnDeactivated;

        if (DataContext is DialogViewModel vm)
            vm.OnCloseRequested -= CloseRequestedFromVm;

        base.OnClosed(e);
    }

    private void CloseRequestedFromVm(object? sender, EventArgs args)
    {
        if (Dispatcher.UIThread.CheckAccess())
            Close();
        else
            Dispatcher.UIThread.Post(Close);
    }
}