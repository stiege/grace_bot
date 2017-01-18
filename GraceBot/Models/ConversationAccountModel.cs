using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GraceBot.Models
{
    public class ConversationAccountModel
    {
        private ConversationAccount _conversationAccount;

        private ConversationAccountModel()
        {
            _conversationAccount = new ConversationAccount();
        }

        public ConversationAccountModel(ConversationAccount conversationAccount)
        {
            _conversationAccount = conversationAccount;
        }

        [Key]
        public string Id
        {
            get { return _conversationAccount.Id; }
            set { _conversationAccount.Id = value; }
        }

        public bool? IsGroup
        {
            get { return _conversationAccount.IsGroup; }
            set { _conversationAccount.IsGroup = value; }
        }

        public string Name
        {
            get { return _conversationAccount.Name; }
            set { _conversationAccount.Name = value; }
        }

        public virtual List<ActivityModel> ActivityModels { get; set; }
    }
}