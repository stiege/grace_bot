using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal interface IExtendedActivity : IConversationUpdateActivity, IContactRelationUpdateActivity, IMessageActivity, ITypingActivity, IEndOfConversationActivity, ITriggerActivity
    {
        IActivity CreateReply(string p0);
    }
}
