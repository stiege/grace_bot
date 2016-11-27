using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal interface IExtendedActivity : IActivity
    {
        IActivity CreateReply(string p0);
        string Text { get; }
    }
}
