using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Serilog;

namespace Motion2Gif.Other;

public interface IFilePickerService
{
    Task<Uri> Pick();
}

public class FilePickerService(Func<TopLevel> getTopLevel) : IFilePickerService
{
    public async Task<Uri> Pick()
    {
        var topLevel = getTopLevel(); 

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open a Video File",
            AllowMultiple = false
        });

        if (files.Count < 1) throw new InvalidOperationException("No files found");
        
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

        return file.Path;
    }
}