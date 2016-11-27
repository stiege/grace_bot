using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal class Factory: IFactory
    {

        public IHttpClient GetHttpClient()
        {
            return new GraceHttpClient(new HttpClient());
        }

        public async Task RespondAsync(string response, IExtendedActivity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync((Activity) activity.CreateReply(response));
        }

        internal IApp GetApp(Activity activity)
        {
            return new App(this, new ExtendedActivity(activity));
        }

        public IFilter GetActivityFilter()
        {
            return new ActivityFilter(this);
        }
    }
}