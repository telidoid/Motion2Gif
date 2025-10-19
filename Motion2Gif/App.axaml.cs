using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
            desktop.MainWindow = new MainWindow();
            
            if (desktop.Args != null)
            {
                foreach (var arg in desktop.Args)
                    Log.Information($"Desktop arg: {arg}");
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}