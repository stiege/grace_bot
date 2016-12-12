namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExtendedActivitiesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExtendedActivities",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Text = c.String(),
                        Type = c.String(),
                        ServiceUrl = c.String(),
                        Timestamp = c.DateTime(),
                        ChannelId = c.String(),
                        Conversation_Id = c.String(maxLength: 128),
                        From_Id = c.String(maxLength: 128),
                        Recipient_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConversationAccounts", t => t.Conversation_Id)
                .ForeignKey("dbo.ChannelAccounts", t => t.From_Id)
                .ForeignKey("dbo.ChannelAccounts", t => t.Recipient_Id)
                .Index(t => t.Conversation_Id)
                .Index(t => t.From_Id)
                .Index(t => t.Recipient_Id);
            
            CreateTable(
                "dbo.ConversationAccounts",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IsGroup = c.Boolean(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChannelAccounts",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExtendedActivities", "Recipient_Id", "dbo.ChannelAccounts");
            DropForeignKey("dbo.ExtendedActivities", "From_Id", "dbo.ChannelAccounts");
            DropForeignKey("dbo.ExtendedActivities", "Conversation_Id", "dbo.ConversationAccounts");
            DropIndex("dbo.ExtendedActivities", new[] { "Recipient_Id" });
            DropIndex("dbo.ExtendedActivities", new[] { "From_Id" });
            DropIndex("dbo.ExtendedActivities", new[] { "Conversation_Id" });
            DropTable("dbo.ChannelAccounts");
            DropTable("dbo.ConversationAccounts");
            DropTable("dbo.ExtendedActivities");
        }
    }
}
