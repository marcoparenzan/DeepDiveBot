using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BotWpfClient.ViewModels
{
    public class RelayCommand : ICommand
    {
        public RelayCommand(Action action)
        {
            this.action = action;
        }

        private Action action;
        private EventHandler canExecuteChanged;

        event EventHandler ICommand.CanExecuteChanged
        {
            add => canExecuteChanged += value;
            remove => canExecuteChanged -= value;
        }

        bool ICommand.CanExecute(object parameter)
        {
            //canExecuteChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            action();
        }
    }
}
