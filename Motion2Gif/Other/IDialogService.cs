using System;
using System.Threading.Tasks;
using Motion2Gif.ViewModels;

namespace Motion2Gif.Other;

public interface IDialogService
{
    public Task<TVm> ShowDialog<TVm>(Action<TVm>? configure = null)
        where TVm : DialogViewModel;
}