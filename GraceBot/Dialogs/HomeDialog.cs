using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using GraceBot.Dialogs;
using System.Threading;

namespace GraceBot.Dialogs
{
    [Serializable]
    internal class HomeDialog : GraceDialog<object>
    {
        #region Fields
        private Dictionary<DialogTypes, List<string>> _responses;
        #endregion

        #region Constructor
        public HomeDialog(IFactory factory) : base(factory)
        {
            _responses = _factory.GetResponseData(
                DialogTypes.Home);
        }
        #endregion

        public override async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activityData = _factory.GetApp().ActivityData;
            await context.PostAsync((await argument).Text);
            await context.PostAsync((activityData.Activity.Text));
            context.Wait(MessageReceivedAsync);
        }
    }
}