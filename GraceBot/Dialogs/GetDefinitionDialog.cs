using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace GraceBot.Dialogs
{
    [Serializable]
    internal class GetDefinitionDialog : GraceDialog<bool>
    {
        #region Fields
        private Dictionary<DialogTypes, List<string>> _responses;
        #endregion

        #region Constructor
        public GetDefinitionDialog(IFactory factory) : base(factory)
        {
            _responses = _factory.GetResponseData(
                DialogTypes.GetDefinition);
        }
        #endregion

        public override async Task StartAsync(IDialogContext context)
        {
            context.Wait<IList<string>>(GetDefinition);
        }

        private async Task GetDefinition(IDialogContext context, IAwaitable<IList<string>> result)
        {
            var def = "";
            foreach (var w in (await result))
            {
                def += w + " ";
            }
            await context.PostAsync("Definition for " + def);

            context.Done(true);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = (await argument).Text;
            if (msg == "back")
                context.Done("Back from GetDefinition");
            else
            {
                var s = msg.Split(' ').ToList();
            }
        }

    }
}