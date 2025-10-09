using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Serilog;

namespace Motion2Gif;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ChooseFile_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);

        if (topLevel == null)
            throw new InvalidOperationException("Top level is null");

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open a Video File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            var file = files[0];
            var fileProperties = await file.GetBasicPropertiesAsync();
            Log.Information($"""
                            
                            File name: {file.Name}
                            File path: {file.Path}
                            Date Created: {fileProperties.DateCreated}
                            Date Modified: {fileProperties.DateModified}
                            Size: {fileProperties.Size} bytes
                            """
            );
        }
    }
}