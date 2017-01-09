using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using GraceBot.Models;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    public class DbManager : IDbManager
    {
        GraceBotContext _db;

        public DbManager(GraceBotContext db)
        {
            _db = db;
        }

        public async Task AddActivity(Activity activity, ProcessStatus processStatus = ProcessStatus.BotMessage)
        {
            await AddOrUpdateActivity(ConvertToModel(activity, processStatus));
        }

        public async Task UpdateActivity(Activity activity)
        {
            await AddOrUpdateActivity(ConvertToModel(activity, null));
        }

        public async Task UpdateActivity(Activity activity, ProcessStatus processStatus)
        {
            await AddOrUpdateActivity(ConvertToModel(activity, processStatus));
        }

        private async Task AddOrUpdateActivity(ActivityModel activityModel)
        {
            // Check if this activity exists in database based on the ActivityId,
            var activityDb = _db.Activities.FirstOrDefault(o => o.ActivityId.Equals(activityModel.ActivityId));
            if (activityDb != null)
            {
                activityModel.Id = activityDb.Id;

                if (activityModel.ProcessStatus == null)
                    activityModel.ProcessStatus = activityDb.ProcessStatus;
            }

            // Check if duplicate key in channelAccount and conversationAccount
            if (activityModel.From!=null)
            {
                var channelAccountFrom = _db.ChannelAccounts.Find(activityModel.From.Id);
                if (channelAccountFrom != null)
                {
                    _db.ChannelAccounts.Attach(channelAccountFrom);
                    activityModel.From = channelAccountFrom;
                }
            }

            if (activityModel.Recipient != null)
            {
                var channelRecipient = _db.ChannelAccounts.Find(activityModel.Recipient.Id);
                if (channelRecipient != null)
                {
                    _db.ChannelAccounts.Attach(channelRecipient);
                    activityModel.Recipient = channelRecipient;
                }
            }

            if (activityModel.Conversation != null)
            {
                var conversationAccount = _db.ConversationAccounts.Find(activityModel.Conversation.Id);
                if (conversationAccount != null)
                {
                    _db.ConversationAccounts.Attach(conversationAccount);
                    activityModel.Conversation = conversationAccount;
                }
            }

            _db.Activities.AddOrUpdate(activityModel);

            await _db.SaveChangesAsync();
        }

        public List<Activity> FindUnprocessedQuestions()
        {
            var extendedActivities = _db.Activities.Include(r => r.From).Include(r => r.Recipient).Include(r => r.Conversation).Where(o => o.ProcessStatus == ProcessStatus.Unprocessed).Take(5).ToList();
            var activities = new List<Activity>();
            foreach (var ea in extendedActivities)
            {
                activities.Add(ConvertToActivity(ea));
            }
            return activities;
        }

        public Activity FindActivity(string id)
        {
            var ea = _db.Activities.Include(r => r.From).Include(r => r.Recipient).Include(r => r.Conversation).FirstOrDefault(o => o.ActivityId == id);
            if (ea==null)
            {
                return null;
            }
            return ConvertToActivity(ea);
        }

        private ActivityModel ConvertToModel(Activity activity, ProcessStatus? processStatus)
        {
            var model = new ActivityModel(activity);
            if (processStatus != null)
                model.ProcessStatus = (ProcessStatus)processStatus;
            return model;
        }

        private Activity ConvertToActivity(ActivityModel activityModel)
        {
            return new Activity()
            {
                Id = activityModel.ActivityId,
                Text = activityModel.Text,
                Type = activityModel.Type,
                ServiceUrl = activityModel.ServiceUrl,
                Timestamp = activityModel.Timestamp,
                ChannelId = activityModel.ChannelId,
                From = activityModel.From,
                Conversation = activityModel.Conversation,
                Recipient = activityModel.Recipient,
                ReplyToId = activityModel.ReplyToId
            };
        }
    }
}