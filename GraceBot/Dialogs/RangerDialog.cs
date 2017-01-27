using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Linq;

namespace GraceBot.Dialogs
{
    [Serializable]
    internal class RangerDialog : GraceDialog, IDialog<object>
    {
        #region Fields
        private int _amount;
        private List<string> _keywords;
        private bool _aborted;
        #endregion

        public RangerDialog(IFactory factory) : base(factory)
        {
            _responses = _factory.GetResponseData(
                DialogTypes.Ranger);
            if (_responses == null)
                throw new InvalidOperationException(
                    $"Get {DialogTypes.Ranger.ToString()} Dialog responses data failed.");
            _aborted = false;
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (_aborted)
            {
                _factory.GetBotManager().SetPrivateConversationDataProperty(
                    "InDialog", DialogTypes.NonDialog);
                _aborted = false;
            }                
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            context.PrivateConversationData.SetValue("InDialog", DialogTypes.Ranger);
            var activityData = _factory.GetApp().ActivityData;
            var msg = (await argument).Text;

            PromptDialog.Choice(context,
                AfterAmount,
                (new long[] { 3, 5, 10 }).AsEnumerable(),
                _responses["SelectAnAmount"][0],
                _responses["RetryInputAmount"][0]
                );

            //await context.PostAsync("In RangerDialog");
            //context.Wait(MessageReceivedAsync);
        }

        private async Task AfterAmount(IDialogContext context, IAwaitable<long> result)
        {
            try
            {
                _amount = (int)await result;
                PromptDialog.Text(context,
                    AfterKeywords,
                    _responses["InputKeywords"][0],
                    _responses["RetryInputKeywords"][0]);
            } catch (Exception)
            {
                _aborted = true;
                context.PostAsync(_responses["AbortRangerDialog"][0]);
                context.Done<object>(null);
            }
        }

        private async Task AfterKeywords(IDialogContext context, IAwaitable<string> result)
        {
            _keywords = (await result).Split(',').ToList();
            for(int i = 0; i < _keywords.Count; i++)
            {
                _keywords[i] = _keywords[i].Trim(' ');
            }

            await PostQuestionsToRanger(context);
            context.Wait(MessageReceivedAsync);
        }

        private async Task PostQuestionsToRanger(IDialogContext context)
        {
            var upq = _factory.GetDbManager().FindUnprocessedQuestions(_amount, _keywords);
            if (upq.Any())
            {
                var attachments = _factory.GetBotManager()
                    .GenerateQuestionsAttachments(upq);
                var reply = context.MakeMessage();
                reply.Attachments = attachments;
                await context.PostAsync(reply);
            }
            else
            {
                await context.PostAsync(_responses["NoUnprocessQuestionsFound"][0]);
            }
        }
    }
}