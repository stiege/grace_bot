using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    public class ExtendedActivity : IExtendedActivity
    {
        private readonly Activity _activity;

        public ExtendedActivity(Activity activity)
        {
            _activity = activity;
        }

        public string Text
        {
            get { return _activity.Text; }
            set { _activity.Text = value; }
        }

        public IActivity CreateReply(string param)
        {
            return _activity.CreateReply(param);
        }

        public string Type
        {
            get { return _activity.Type; }
            set { _activity.Type = value; }
        }

        public string Id
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

        public ChannelAccount From
        {
            get { return _activity.From; }
            set { _activity.From = value; }
        }

        public ConversationAccount Conversation
        {
            get { return _activity.Conversation; }
            set { _activity.Conversation = value; }
        }

        public ChannelAccount Recipient
        {
            get { return _activity.Recipient; }
            set { _activity.Recipient = value; }
        }

        /// <summary>
        /// This property indicates whether this activity has been processed or not,
        /// It's Enum Type, and the value, instead of literal string, will be saved to database
        /// </summary>
        [EnumDataType(typeof(ProcessStatus))]
        public ProcessStatus ProcessStatus { get; set; }
    }



    public enum ProcessStatus
    {
        BotReplied = 1,
        Unprocessed = 2,
        Processed = 3
    };
}