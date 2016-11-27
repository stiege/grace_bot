using System;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal static class Factory
    {
        internal static IFilter GetActivityFilter()
        {
            return new ActivityFilter();
        }

        internal static IApp GetApp(Activity activity)
        {
            return new App(new ExtendedActivity(activity));
        }
    }
}