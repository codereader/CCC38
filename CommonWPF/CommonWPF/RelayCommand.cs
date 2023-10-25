using System;
using System.Windows.Input;

namespace CommonWPF;

public class RelayCommand : ICommand
{
    private readonly Predicate<object> _canExecute;
    private readonly Action<object> _execute;

    public RelayCommand(Predicate<object> canExecute, Action<object> execute)
    {
        _canExecute = canExecute;
        _execute = execute;
    }
    
    public RelayCommand(Func<bool> canExecute, Action execute) :
        this((object _) =>
        canExecute(), 
            (object _) => execute())
    {}
    
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
