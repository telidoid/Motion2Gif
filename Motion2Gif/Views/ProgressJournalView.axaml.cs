using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Motion2Gif.ViewModels;
using Serilog;

namespace Motion2Gif.Views;

public partial class ProgressJournalView : UserControl
{
    public ProgressJournalView()
    {
        InitializeComponent();
        DataContext = Program.AppHost.Services.GetRequiredService<ProgressJournalViewModel>();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (sender is Control ctl)
                FlyoutBase.ShowAttachedFlyout(ctl);
        });
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Log.Information("Click");
    }
}