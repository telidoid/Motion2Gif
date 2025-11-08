using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Motion2Gif.Other;
using Motion2Gif.Player;
using Motion2Gif.Processing;
using Motion2Gif.ViewModels;
using Motion2Gif.Views;
using Motion2Gif.VLC;

namespace Motion2Gif;

internal static class DependencyInjectionExtensions
{
    public static IHostBuilder ConfigureMotion2Gif(this IHostBuilder builder)
        => builder.ConfigureServices(services =>
        {
            services.AddTransient<IVideoPlayerService, VideoPlayerService>();
            services.AddSingleton<IFilePickerService, FilePickerService>();
            services.AddSingleton<IJobProcessingService, JobProcessingService>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<ProgressJournalViewModel>();
            
            services.AddSingleton<Func<IStorageProvider>>(_ => () =>
            {
                var lifetime = Application.Current?.ApplicationLifetime;
                
                if (lifetime == null)
                    throw new NullReferenceException("Application Lifetime is null");

                if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var win = desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow;
                    return win is null ? throw new InvalidOperationException("No active window.") : win.StorageProvider;
                }

                throw new InvalidOperationException($"Unsupported lifetime: {lifetime.GetType()}");
            });

            services.AddSingleton<Func<Window>>(_ => ()=>
            {
                var lifetime = Application.Current?.ApplicationLifetime;

                if (lifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                    throw new InvalidOperationException("No active window.");
                
                var win = desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow;
                return win ?? throw new InvalidOperationException("No active window.");
            });
            
            // Dialogs
            services.AddSingleton<IDialogService, DialogService>();
            services.AddTransient<ExportDialogViewModel>();
            services.AddTransient<DialogWindow>(); // one view == one view model, i.e. they should be transient
        });
}