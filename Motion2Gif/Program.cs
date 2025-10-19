using Avalonia;
using System;
using System.IO;
using Avalonia.Logging;
using Serilog;
using LogEventLevel = Serilog.Events.LogEventLevel;

namespace Motion2Gif;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
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