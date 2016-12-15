using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Net.Http;

namespace GraceBot
{
    internal interface IExtendedActivity : IActivity
    {
        IActivity CreateReply(string p0);
        string Text { get; }
        StateClient GetStateClient(string microsoftAppId = null, string microsoftAppPassword = null, string serviceUrl = null, params DelegatingHandler[] handlers);
    }
}
