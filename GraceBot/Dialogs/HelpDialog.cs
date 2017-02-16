using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace GraceBot.Dialogs
{
    [Serializable]
    internal class HelpDialog : GraceDialog, IDialog<IDialogResult>
    {
        #region Configurations
        private const DialogTypes TYPE = DialogTypes.Help;

        private static readonly List<string> PROPERTY_USED = new List<string>
        { };
        #endregion
        public HelpDialog(IFactory factory, IResponseManager responses)
            : base(TYPE, PROPERTY_USED, factory, responses)
        { }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(
            IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument;
            var text = activity.Text?.ToLower();

            PromptDialog.Choice(context,
                AfterSelection,
                new string[] { "OMGTech", "Grace Bot" },
               _responses.GetResponseByKey("SelectTopic"),
               _responses.GetResponseByKey("RetryTopic")
               );
        }

        private async Task AfterSelection(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var topic = await result;
                var answer = _responses.GetResponseByKey(topic);
                await context.PostAsync(answer);
            }
            catch (TooManyAttemptsException)
            {
                context.PostAsync("Abort help.");
            }
            ReturnToParentDialog(context);
        }
    }
}