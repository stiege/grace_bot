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
        private static readonly List<string> PROPERTY_USED = new List<string> { };

        internal RootDialog(IFactory factory, IResponseManager responses)
            : base(DialogTypes.Root, PROPERTY_USED, factory, responses)
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
                case DialogTypes.NoneDialog:
                    {
                        context.PostAsync("Sorry, unexpected errors.");
                        ResetDialog(context, PROPERTY_USED);
                        break;
                    }
                default:
                    {
                        await context.Forward(_factory.MakeIDialog(inDialog),
                            AfterChildDialog,
                            await argument,
                            new CancellationTokenSource().Token);
                        break;
                    }
            }
        }

        private async Task AfterChildDialog(IDialogContext context, IAwaitable<IDialogResult> result)
        {
            var resultInstance = await result;
            ResetDialog(context, resultInstance.PropertiesUsed);
        }

        private IList<string> ResetDialog(IDialogContext context, IList<string> propertiesToBeRemoved)
        {
            context.PrivateConversationData.SetValue("InDialog", DialogTypes.NoneDialog);
            propertiesToBeRemoved.Add("Command");

            var failedToRemove = new List<string>();
            foreach (var p in propertiesToBeRemoved)
            {
                if (!context.PrivateConversationData.RemoveValue(p))
                    failedToRemove.Add(p);
            }
            context.Done(new object());
            return failedToRemove;
        }
    }
}