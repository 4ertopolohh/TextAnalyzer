using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Windows.Input;

namespace TextAnalyzer.ViewModels
{
    public class RelayCommand : ICommand
    {
        //делегат который надо выполнить при вызове команды, принимает метод из MainWindiwViewModel
        private readonly Action<object> _execute;
        //делегат проверка, если фолс то кнопка неактивна
        private readonly Func<object, bool> _canExecute;

        //конструктор
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            //проверка на нулл
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        //событие проверки сделать ли кнопку активной
        public event EventHandler CanExecuteChanged
        {
            //встроенное событие
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        //если передан нулл 
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        //выполняет метод из MainWindiwViewModel
        public void Execute(object parameter) => _execute(parameter);
    }
}
