using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Motion2Gif.ViewModels;
using Motion2Gif.Views;

namespace Motion2Gif.Other;

public class DialogService(IServiceProvider serviceProvider, Func<Window?> getOwnerFunc) : IDialogService
{
    public async Task<TVm> ShowDialog<TVm>(Action<TVm>? configure = null)
        where TVm : DialogViewModel
    {
        var owner = getOwnerFunc() ?? throw new NullReferenceException("No owner window");
        var vm = serviceProvider.GetRequiredService<TVm>();
        configure?.Invoke(vm);

        var dialogWindow = serviceProvider.GetRequiredService<DialogWindow>();
        dialogWindow.DataContext = vm;

        await dialogWindow.ShowDialog(owner);
        return vm;
    }
}