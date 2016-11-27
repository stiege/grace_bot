using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal class ActivityFilter : IFilter
    {
        public async Task<bool> FilterAsync(IExtendedActivity activity)
        {
            if (
                !File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\BadWords\\en")
                    .Any(badWord => activity.Text.ToLower().Contains(badWord.ToLower())))
            {
                return await Task.FromResult(true);
            }
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync((Activity)activity.CreateReply("..."));
            return await Task.FromResult(false);
        }
    }
}