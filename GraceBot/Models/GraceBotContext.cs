﻿using System;
using System.Data.Entity;
using Microsoft.Bot.Connector;

namespace GraceBot.Models
{
    public class GraceBotContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public GraceBotContext() : base("name=GraceBotContext")
        {
        }         

        public virtual DbSet<ActivityModel> Activities { get; set; }
        public virtual DbSet<ChannelAccountModel> ChannelAccounts { get; set; }
        public virtual DbSet<ConversationAccountModel> ConversationAccounts { get; set; }
        public virtual DbSet<UserAccount> UserAccounts { get; set; }
        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<AnswerRating> AnswerRatings { get; set; }
        public virtual DbSet<TwitterQuestion> TwitterQuestions { get; set; }
    }

}
