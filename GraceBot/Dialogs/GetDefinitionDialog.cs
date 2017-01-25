using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace GraceBot.Dialogs
{
    internal class GetDefinitionDialog : GraceDialog<IMessageActivity>
    {
        private Dictionary<string, List<string>> _responses;

        public const string NAME = "GetDefinition";

        public GetDefinitionDialog(IFactory factory, params object[] dialogVariables) : base(factory, dialogVariables)
        {
            _responses = _factory.GetResponseData(NAME);
        }

        public override async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            throw new NotImplementedException();
        }

    }
}