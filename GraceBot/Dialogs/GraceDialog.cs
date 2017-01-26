using GraceBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot.Dialogs
{
    [Serializable]
    internal abstract class GraceDialog<R> : IDialog<R>
    {
        protected IFactory _factory;
        
        private GraceDialog() { }
        protected GraceDialog(IFactory factory)
        {
            _factory = factory;
        }
        
        public abstract Task StartAsync(IDialogContext context);
    }

    public enum DialogTypes
    {
        Home,
        GetDefinition
    }
}
