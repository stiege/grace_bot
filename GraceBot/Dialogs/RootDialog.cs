using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Threading;

namespace GraceBot.Dialogs
{
    [Serializable]
    internal class RootDialog : GraceDialog, IDialog<object>
    {
        internal RootDialog(IFactory factory, IResponseManager responses)
            : base(factory, responses)
        { }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            DialogTypes inDialog;
            if (!context.PrivateConversationData.TryGetValue("InDialog", out inDialog))
            {
                context.PostAsync("Sorry, unexpected errors.");
                context.Wait(MessageReceivedAsync);
                return;
            }
            switch(inDialog)
            {
                case DialogTypes.Ranger:
                    {
                        await context.Forward(
                            _factory.MakeIDialog<bool>(inDialog),
                            AfterRangerDialog,
                            await argument,
                            new CancellationTokenSource().Token);
                        break;
                    }
                case DialogTypes.Help:
                    {
                        await context.Forward(
                            _factory.MakeIDialog<object>(inDialog),
                            AfterHelpDialog,
                            await argument,
                            new CancellationTokenSource().Token);
                        break;
                    }
                case DialogTypes.NonDialog:
                    {
                        goto default;
                    }
                default:
                    {
                        context.PostAsync("Sorry, unexpected errors.");
                        context.Wait(MessageReceivedAsync);
                        break;
                    }
            }
        }

        private Task AfterRangerDialog(IDialogContext context, IAwaitable<bool> result)
        {
            context.PrivateConversationData.RemoveValue("QuestionActivity");
            context.PrivateConversationData.RemoveValue("AnswerActivity");
            ResetDialog(context);
            return Task.CompletedTask;
        }

        private Task AfterHelpDialog(IDialogContext context, IAwaitable<object> result)
        {
            ResetDialog(context);
            return Task.CompletedTask;
        }

        private void ResetDialog(IDialogContext context)
        {
            context.PrivateConversationData.SetValue("InDialog", DialogTypes.NonDialog);
            context.Done(new object());
        }
    }
}