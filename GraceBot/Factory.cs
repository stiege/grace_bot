using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal class Factory: IFactory
    {
        private static IFactory _factoryInstance;
        private static IApp _appInstance;
        private Factory()
        { }

        internal static IFactory GetFactory()
        {
            _factoryInstance = _factoryInstance ?? new Factory();
            return _factoryInstance;
        }

        public IHttpClient GetHttpClient()
        {
            return new GraceHttpClient(new HttpClient());
        }

        public async Task RespondAsync(string response, IExtendedActivity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync((Activity) activity.CreateReply(response));
        }

        public IApp GetApp()
        {
            _appInstance = _appInstance ?? new App(GetFactory());
            return _appInstance;
        }

        public IFilter GetActivityFilter()
        {
            var sep = Path.DirectorySeparatorChar;
            return new ActivityFilter(this, File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + $"{sep}BadWords{sep}en"));
        }
    }
}