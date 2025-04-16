using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UPApp.Models
{
    public class CommanAction : ICommand
    {
        Action action;
        Action<object> actionObj;
        public CommanAction(Action action)
        {
            this.action = action;
        }
        public CommanAction(Action<object> actionObj)
        {
            this.actionObj = actionObj;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (actionObj != null)
            {
                return parameter != null;
            }
            return true;
        }

        public void Execute(object parameter)
        {
            action?.Invoke();
            actionObj?.Invoke(parameter);
        }
    }
}
