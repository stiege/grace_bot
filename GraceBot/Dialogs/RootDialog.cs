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
                await context.PostAsync("Sorry, unexpected errors. It seems that you are not in any Dialog.");
                context.Done(new object());
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
                case DialogTypes.Answer:
                    {
                        await context.Forward(
                            _factory.MakeIDialog<object>(inDialog),
                            AfterAnswerDialog,
                            await argument,
                            new CancellationTokenSource().Token);
                        break;
                    }
                case DialogTypes.RateAnswer:
                    {
                        await context.Forward(
                            _factory.MakeIDialog<object>(inDialog),
                            AfterRateAnswerDialog,
                            await argument,
                            new CancellationTokenSource().Token);
                        break;
                    }
                case DialogTypes.NoneDialog:
                    {
                        goto default;
                    }
                default:
                    {
                        context.PostAsync("Sorry, unexpected errors.");
                        ResetDialog(context);
                        break;
                    }
            }
        }

        private Task AfterAnswerDialog(IDialogContext context, IAwaitable<object> result)
        {
            ResetDialog(context);
            return Task.CompletedTask;
        }

        private Task AfterRateAnswerDialog(IDialogContext context, IAwaitable<object> result)
        {
            ResetDialog(context, "SubjectOfAnswer",
                "AnswerRate", "AnswerActivity",
                "RatingActivity");
            return Task.CompletedTask;
        }

        private Task AfterRangerDialog(IDialogContext context, IAwaitable<bool> result)
        {
            ResetDialog(context, "QuestionActivity", "AnswerActivity");
            return Task.CompletedTask;
        }

        private Task AfterHelpDialog(IDialogContext context, IAwaitable<object> result)
        {
            ResetDialog(context);
            return Task.CompletedTask;
        }

        private void ResetDialog(IDialogContext context, params string[] propertyNames)
        {
            context.PrivateConversationData.SetValue("InDialog", DialogTypes.NoneDialog);
            var propertyList = propertyNames.ToList();
            propertyList.Add("Command");

            var failedToRemove = new List<string>();
            foreach (var p in propertyList)
            {
                if (!context.PrivateConversationData.RemoveValue(p))
                    failedToRemove.Add(p);
            }
            context.Done(new object());
        }
    }
}