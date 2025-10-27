using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Motion2Gif.Other;
using Motion2Gif.Player;
using Motion2Gif.Processing;
using Motion2Gif.ViewModels;
using Motion2Gif.VLC;

namespace Motion2Gif;

internal static class DependencyInjectionExtensions
{
    public static IHostBuilder ConfigureMotion2Gif(this IHostBuilder builder)
        => builder.ConfigureServices(services =>
        {
            services.AddSingleton<IVideoPlayerService, VideoPlayerService>();
            services.AddSingleton<IFilePickerService, FilePickerService>();
            services.AddSingleton<IJobProcessingService, JobProcessingService>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<Func<IStorageProvider>>(_ => () =>
            {
                var lifetime = Application.Current?.ApplicationLifetime;

                if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var win = desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow;
                    return win is null ? throw new InvalidOperationException("No active window.") : win.StorageProvider;
                }

                if (lifetime is ISingleViewApplicationLifetime single)
                {
                    var top = single.MainView?.GetVisualRoot() as TopLevel ??
                              throw new InvalidOperationException("No VisualRoot.");
                    return top.StorageProvider;
                }

                throw new InvalidOperationException("Unknown lifetime.");
            });
        });
}