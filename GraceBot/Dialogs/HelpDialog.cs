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
    internal class HelpDialog : GraceDialog, IDialog<object>
    {
        public HelpDialog(IFactory factory, IResponseManager responses) : base(factory, responses)
        {
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(
            IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            context.PrivateConversationData.SetValue("InDialog", DialogTypes.Help);

            var activity = await argument;
            //if (!string.IsNullOrEmpty(activity.Text) && activity.Text.ToLower() != CommandString.HELP.ToLower())
            //{
            //    await PostResult(context, activity.Text);
            //    return;
            //}

            if (!string.IsNullOrEmpty(activity.Text))
            {
                if (activity.Text!="help"&&!_responses.ContainsKey(activity.Text))
                {
                    ResetDialog(context);
                    return ;
                }
            }

            PromptDialog.Choice(context,
                AfterSelection,
                new string[] { "OMGTech", "Grace Bot", "..." },
               _responses.GetResponseByKey("SelectTopic"),
               _responses.GetResponseByKey("RetryTopic")
               );
        }

        private async Task AfterSelection(IDialogContext context, IAwaitable<object> result)
        {
            var selectedTopic = (string)(await result);

            await PostResult(context, selectedTopic);
        }

        private async Task PostResult(IDialogContext context, string topic)
        {
            if (!_responses.ContainsKey(topic))
            {
                ResetDialog(context);
                return;
            }
                var answer = _responses.GetResponseByKey(topic);
                await context.PostAsync(answer);

        }

        private void ResetDialog(IDialogContext context)
        {
            context.PrivateConversationData.SetValue("InDialog", DialogTypes.NonDialog);
            context.Done<object>(null);
        }

    }
}