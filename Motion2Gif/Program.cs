using Avalonia;
using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Serilog;
using LogEventLevel = Serilog.Events.LogEventLevel;

namespace Motion2Gif;

internal static class Program
{
    public static IHost AppHost { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        AppHost = Host.CreateDefaultBuilder(args)
            .ConfigureMotion2Gif()
            .Build();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .ConfigureAppLogging()
            .LogToTrace();

    private static AppBuilder ConfigureAppLogging(this AppBuilder appBuilder)
    {
        var logsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Motion2Gif", "Logs");

        Directory.CreateDirectory(logsDir);

        var logFile = Path.Combine(logsDir, "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                path: logFile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                rollOnFileSizeLimit: true,
                restrictedToMinimumLevel: LogEventLevel.Information
            )
            .CreateLogger();

        return appBuilder;
    }
}