using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    interface ICommand
    {
        Task Execute(Activity activity);
    }

    public static class CommandString
    {
        public const string CMD_PREFIX = "//";
        public const string GET_UNPROCESSED_QUESTIONS = CMD_PREFIX + "get";
        public const string REPLYING_TO_QUESTION = CMD_PREFIX + "replyActivity";
    }
}
