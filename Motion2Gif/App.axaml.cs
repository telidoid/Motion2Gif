using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Motion2Gif.Other;
using Motion2Gif.ViewModels;
using Motion2Gif.Views;
using Serilog;

namespace Motion2Gif;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var sp = Program.AppHost.Services;
            
            desktop.MainWindow = new MainWindow()
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>() 
            };
            
            if (desktop.Args != null)
            {
                foreach (var arg in desktop.Args)
                {
                    var vm = desktop.MainWindow.DataContext as MainWindowViewModel;
                    _ = Dispatcher.UIThread.InvokeAsync(() => vm!.OpenVideoFile(arg));
                }
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}