using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace GraceBot.Dialogs
{
    internal class HomeDialog : IDialog<object>
    {
        private IFactory _factory;
        private Dictionary<string, List<string>> _responses;

        public const string Name = "Home";

        public HomeDialog(IFactory factory)
        {
            _factory = factory;
            _responses = _factory.GetResponseData(Name);
        }
        private HomeDialog() { }
        
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> argument)
        {
            throw new NotImplementedException();
        }
    }
}