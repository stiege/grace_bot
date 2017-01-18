using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceBot.Models
{
    public class ActivityModel
    {
        private Activity _activity;

        private ActivityModel()
        {
            _activity = new Activity();
        }

        public ActivityModel(Activity activity, ChannelAccountModel from = null, 
            ChannelAccountModel recipient = null, ConversationAccountModel conversation = null)
        {
            _activity = activity;
            Id = Guid.NewGuid().ToString();

            From = from ?? new ChannelAccountModel(_activity.From);
            Recipient = recipient ?? new ChannelAccountModel(_activity.Recipient);
            Conversation = conversation ?? new ConversationAccountModel(_activity.Conversation);
        }

        [Key]
        public string Id { get; private set; }

        public string Text
        {
            get { return _activity.Text; }
            set { _activity.Text = value; }
        }


        public string Type
        {
            get { return _activity.Type; }
            set { _activity.Type = value; }
        }

        public string ActivityId
        {
            get { return _activity.Id; }
            set { _activity.Id = value; }
        }

        public string ServiceUrl
        {
            get { return _activity.ServiceUrl; }
            set { _activity.ServiceUrl = value; }
        }

        public DateTime? Timestamp
        {
            get { return _activity.Timestamp; }
            set { _activity.Timestamp = value; }
        }

        public string ChannelId
        {
            get { return _activity.ChannelId; }
            set { _activity.ChannelId = value; }
        }

        public string FromId
        {
            get; set;
        }

        public string ConversationId
        {
            get; set;
        }

        public string RecipientId
        {
            get; set;
        }

        public string ReplyToId
        {
            get { return _activity.ReplyToId; }
            set { _activity.ReplyToId = value; }
        }

        /// <summary>
        /// This property indicates whether this activity has been processed or not,
        /// It's Enum Type, and the value, instead of literal string, will be saved to database
        /// </summary>
        [EnumDataType(typeof(ProcessStatus))]
        public ProcessStatus ProcessStatus { get; set; }

        public virtual ChannelAccountModel From { get; set; }
        public virtual ChannelAccountModel Recipient { get; set; }
        public virtual ConversationAccountModel Conversation { get; set; }
    }

    public enum ProcessStatus
    {
        BotReplied = 1,
        Unprocessed = 2,
        Processed = 3,
        BotMessage = 4,
    };
}