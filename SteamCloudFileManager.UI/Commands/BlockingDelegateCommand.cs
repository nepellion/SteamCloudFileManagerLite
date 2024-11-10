using System;
using System.Threading.Tasks;
using Prism.Commands;

namespace SteamCloudFileManager.UI.Commands;

public class BlockingDelegateCommand(Func<object, Task> execute, Func<object, bool>? canExecute = null) : DelegateCommandBase
{
    Task? runningTask;
    
    public BlockingDelegateCommand(Func<Task> execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute is not null ? _ => canExecute() : null)
    {
    }

    protected override void Execute(object parameter)
    {
        if (runningTask is not null && !runningTask.IsCompleted)
            return;
            
        runningTask = execute(parameter);
    }

    protected override bool CanExecute(object parameter)
        => (runningTask?.IsCompleted ?? true)
        && (canExecute?.Invoke(parameter) ?? true);
}