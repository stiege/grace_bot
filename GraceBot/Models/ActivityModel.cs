using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceBot.Models
{
    public class ActivityModel
    {
        private  Activity _activity;

        private ActivityModel()
        {
            _activity = new Activity();
        }

        public ActivityModel(Activity activity)
        {
            _activity = activity;
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; }

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

        [Index("IX_ActivityId", IsUnique = true)]
        [MaxLength(64)]
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

        public virtual ChannelAccount From
        {
            get { return _activity.From; }
            set { _activity.From = value;
            }
        }
        public virtual ChannelAccount Recipient
        {
            get { return _activity.Recipient; }
            set { _activity.Recipient = value;
            }
        }
        public virtual ConversationAccount Conversation
        {
            get { return _activity.Conversation; }
            set { _activity.Conversation = value; }
        }

    }

    public enum ProcessStatus
    {
        BotReplied = 1,
        Unprocessed = 2,
        Processed = 3,
        BotMessage = 4,
    };
}