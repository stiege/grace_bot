using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using GraceBot.Models;

namespace GraceBot.Controllers
{
    public static class DbController
    {
        private static GraceBotContext db = new GraceBotContext();

        public static async Task AddOrUpdateActivityInDb(ExtendedActivity extendedActivity, ProcessStatus processStatus)
        {
            // Check if duplicate key in channelAccount and conversationAccount

            var channelAccountFrom = await db.ChannelAccounts.FindAsync(extendedActivity.From.Id);
            var channelRecipient = await db.ChannelAccounts.FindAsync(extendedActivity.Recipient.Id);
            var conversationAccount = await db.ConversationAccounts.FindAsync(extendedActivity.Conversation.Id);

            if (channelAccountFrom != null)
            {
                db.ChannelAccounts.Attach(channelAccountFrom);
                extendedActivity.From = channelAccountFrom;
            }
            if (channelRecipient != null)
            {
                db.ChannelAccounts.Attach(channelRecipient);
                extendedActivity.Recipient = channelRecipient;
            }

            if (conversationAccount != null)
            {
                db.ConversationAccounts.Attach(conversationAccount);
                extendedActivity.Conversation = conversationAccount;
            }


            // Set processStatus
            extendedActivity.ProcessStatus = processStatus;

            // Save extendedActivity to database
            db.ExtendedActivities.AddOrUpdate(extendedActivity);
            await db.SaveChangesAsync();
        }

        public static List<ExtendedActivity> FindUnprocessedQuestions()
        {
            return db.ExtendedActivities.Include(r=>r.From).Include(r=>r.Recipient).Include(r=>r.Conversation).Where(o => o.ProcessStatus == ProcessStatus.Unprocessed).Take(5).ToList();
        }

        public static ExtendedActivity FindExtendedActivity(string id)
        {
            return db.ExtendedActivities.FirstOrDefault(o => o.Id == id);
        }
    }
}