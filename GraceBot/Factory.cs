using System;
using System.Net.Http;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal class Factory: IFactory
    {

        public IHttpClient GetHttpClient()
        {
            return new GraceHttpClient(new HttpClient());
        }

        internal IApp GetApp(Activity activity)
        {
            return new App(this, new ExtendedActivity(activity));
        }

        public IFilter GetActivityFilter()
        {
            return new ActivityFilter();
        }
    }
}