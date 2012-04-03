using System;
using System.Windows.Input;

namespace TimeTable
{
   public class ActionCommand : ICommand
   {
      readonly Action<object> _execute;
      readonly Predicate<object> _canExecute;

      public ActionCommand(Action<object> execute)
         : this(execute, null)
      {
      }

      public ActionCommand(Action<object> execute, Predicate<object> canExecute)
         : this(execute, canExecute, null)
      {
      }

      public ActionCommand(Action<object> execute, Predicate<object> canExecute, EventHandler canExecuteChangedMethod)
      {
         if (execute == null)
            throw new ArgumentNullException("execute");

         _execute = execute;
         _canExecute = canExecute;
      }

      public bool CanExecute(object parameter)
      {
         return _canExecute == null || _canExecute(parameter);
      }

      public event EventHandler CanExecuteChanged
      {
         add
         {
            CommandManager.RequerySuggested += value;
         }
         remove
         {
            CommandManager.RequerySuggested -= value;
         }
      }

      public void Execute(object parameter)
      {
         _execute(parameter);
      }


   }
}
