using System;
using System.Windows.Input;

namespace DesktopRecord.Helper
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object, bool> canExecuteFunc;
        private readonly Action<object> executeAction;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null) return;
            executeAction = execute;
            canExecuteFunc = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteFunc == null) return true;
            return canExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            if (executeAction == null) return;
            executeAction(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
