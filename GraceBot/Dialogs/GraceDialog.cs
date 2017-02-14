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
    internal abstract class GraceDialog
    {
        protected IFactory _factory;
        protected IResponseManager _responses;

        private GraceDialog() { }
        protected GraceDialog(IFactory factory, IResponseManager responses)
        {
            _factory = factory;
            _responses = responses;
        }
    }

    public enum DialogTypes
    {
        NonDialog = 0,

        Root,
        Help,
        Home,
        GetDefinition,
        Ranger,
        RateAnswer,
    }
}
