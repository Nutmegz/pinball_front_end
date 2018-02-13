using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinballFrontEnd.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        //Default Can Execute Constructor
        public RelayCommand(Action<object> execute) : this(execute, null)
        {

        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            //Default Can Execute
            if (_canExecute == null)
                return true;

            return _canExecute((object)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((object)parameter);
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }


    }
}
