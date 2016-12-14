using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GraceBot.Models;
using Microsoft.Bot.Connector;

namespace GraceBot.Controllers
{
    public static class DbController
    {
        private static GraceBotContext db = new GraceBotContext();

        public  static async void SaveActivityToDb(Activity activity)
        {
            // Check if duplicate key in channelAccount and conversationAccount
            if (db.ChannelAccounts.Any())
            {
                if (db.ChannelAccounts.Select(c => c.Id).Contains(activity.From.Id))
                {
                    db.ChannelAccounts.Attach(activity.From);
                }
                if (db.ChannelAccounts.Select(c => c.Id).Contains(activity.Recipient.Id))
                {
                    db.ChannelAccounts.Attach(activity.Recipient);
                }
            }

            if (db.ConversationAccounts.Any())
            {
                if (db.ConversationAccounts.Select(c => c.Id).Contains(activity.Conversation.Id))
                {
                    db.ConversationAccounts.Attach(activity.Conversation);
                }
            }

            // Save activity to database
            db.ExtendedActivities.Add(activity);
            await db.SaveChangesAsync();
        }
    }
}