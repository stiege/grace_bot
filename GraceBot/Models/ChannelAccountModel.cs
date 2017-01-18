using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GraceBot.Models
{
    public class ChannelAccountModel
    {
        private ChannelAccount _channelAccount;

        private ChannelAccountModel()
        {
            _channelAccount = new ChannelAccount();
        }

        public ChannelAccountModel(ChannelAccount channelAccount, UserAccount userAccount = null)
        {
            _channelAccount = channelAccount;
            UserAccount = userAccount;
        }

        [Key]
        public string Id
        {
            get { return _channelAccount.Id; }
            set { _channelAccount.Id = value; }
        }

        public string Name
        {
            get { return _channelAccount.Name; }
            set { _channelAccount.Name = value; }
        }

        public string UserAccountId { get; set; }

        public virtual List<ActivityModel> ActivityModels { get; set; }
        public virtual UserAccount UserAccount { get; set; }
    }
}