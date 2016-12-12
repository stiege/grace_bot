using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GraceBot.Models;

namespace GraceBot.Controllers
{
    public static class DbController
    {
        private static GraceBotContext db = new GraceBotContext();

        public  static async void SaveActivityToDb(ExtendedActivity extendedActivity, ProcessStatus processStatus)
        {
            // Check if duplicate key in channelAccount and conversationAccount
            if (db.ChannelAccounts.Any())
            {
                if (db.ChannelAccounts.Select(c => c.Id).Contains(extendedActivity.From.Id))
                {
                    db.ChannelAccounts.Attach(extendedActivity.From);
                }
                if (db.ChannelAccounts.Select(c => c.Id).Contains(extendedActivity.Recipient.Id))
                {
                    db.ChannelAccounts.Attach(extendedActivity.Recipient);
                }
            }

            if (db.ConversationAccounts.Any())
            {
                if (db.ConversationAccounts.Select(c => c.Id).Contains(extendedActivity.Conversation.Id))
                {
                    db.ConversationAccounts.Attach(extendedActivity.Conversation);
                }
            }

            // Set processStatus
            extendedActivity.ProcessStatus = processStatus;

            // Save extendedActivity to database
            db.ExtendedActivities.Add(extendedActivity);
            await db.SaveChangesAsync();
        }
    }
}