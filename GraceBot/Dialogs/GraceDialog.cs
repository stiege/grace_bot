using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot
{
    internal abstract class GraceDialog<T> : IDialog<T>
    {
        protected IFactory _factory;
        protected object[] _dialogVariables;

        private GraceDialog() { }
        protected GraceDialog(IFactory factory, params object[] dialogVariables)
        {
            _factory = factory;
            _dialogVariables = dialogVariables;
        }
        
        internal virtual void SetDialogVariables(params object[] dialogVariables)
        {
            _dialogVariables = dialogVariables;
        }

        public abstract Task StartAsync(IDialogContext context);
    }
}
