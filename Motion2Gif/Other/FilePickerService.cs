using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Serilog;

namespace Motion2Gif.Other;

public interface IFilePickerService
{
    Task<Uri?> Pick();
}

public class FilePickerService(Func<IStorageProvider> storageFactory) : IFilePickerService
{
    public async Task<Uri?> Pick()
    {
        var storageProvider = storageFactory();
        
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open a Video File",
            AllowMultiple = false
        });

        if (!files.Any())
            return null;
        
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