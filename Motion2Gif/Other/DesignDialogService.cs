using System;
using System.Threading.Tasks;
using Motion2Gif.ViewModels;

namespace Motion2Gif.Other;

public class DesignDialogService: IDialogService
{
    public Task<TVm> ShowDialog<TVm>(Action<TVm>? configure = null) where TVm : DialogViewModel
    {
        return (Task<TVm>)Task.CompletedTask;
    }
}