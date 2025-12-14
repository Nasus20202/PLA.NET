using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProcessMonitor.Commands;

public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private readonly Action<string> _errorHandler;
    private Task? _executingTask;

    public AsyncRelayCommand(
        Func<object?, Task> execute,
        Func<object?, bool>? canExecute = null,
        Action<string>? errorHandler = null
    )
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _errorHandler =
            errorHandler
            ?? (
                message =>
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            );
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) =>
        (_executingTask == null || _executingTask.IsCompleted)
        && (_canExecute == null || _canExecute(parameter));

    public void Execute(object? parameter)
    {
        _executingTask = ExecuteAsync(parameter);
    }

    private async Task ExecuteAsync(object? parameter)
    {
        try
        {
            await _execute(parameter);
        }
        catch (UnauthorizedAccessException)
        {
            _errorHandler("Access denied.");
        }
        catch (InvalidOperationException ex)
        {
            _errorHandler($"Operation failed: {ex.Message}");
        }
    }
}

public class AsyncRelayCommand<T> : ICommand
{
    private readonly Func<T?, Task> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private readonly Action<string> _errorHandler;
    private Task? _executingTask;

    public AsyncRelayCommand(
        Func<T?, Task> execute,
        Func<T?, bool>? canExecute = null,
        Action<string>? errorHandler = null
    )
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _errorHandler =
            errorHandler
            ?? (
                message =>
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            );
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) =>
        (_executingTask == null || _executingTask.IsCompleted)
        && (_canExecute == null || _canExecute((T?)parameter));

    public void Execute(object? parameter)
    {
        _executingTask = ExecuteAsync((T?)parameter);
    }

    private async Task ExecuteAsync(T? parameter)
    {
        try
        {
            await _execute(parameter);
        }
        catch (UnauthorizedAccessException)
        {
            _errorHandler("Access denied.");
        }
        catch (InvalidOperationException ex)
        {
            _errorHandler($"Operation failed: {ex.Message}");
        }
    }
}
