namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitNewSchemaDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActivityModels",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Text = c.String(),
                        Type = c.String(),
                        ActivityId = c.String(maxLength: 64),
                        ServiceUrl = c.String(),
                        Timestamp = c.DateTime(),
                        ChannelId = c.String(),
                        FromId = c.String(maxLength: 128),
                        ConversationId = c.String(maxLength: 128),
                        RecipientId = c.String(maxLength: 128),
                        ReplyToId = c.String(),
                        ProcessStatus = c.Int(nullable: false),
                        ChannelAccountModel_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConversationAccountModels", t => t.ConversationId)
                .ForeignKey("dbo.ChannelAccountModels", t => t.ChannelAccountModel_Id)
                .ForeignKey("dbo.ChannelAccountModels", t => t.FromId)
                .ForeignKey("dbo.ChannelAccountModels", t => t.RecipientId)
                .Index(t => t.ActivityId, unique: true)
                .Index(t => t.FromId)
                .Index(t => t.ConversationId)
                .Index(t => t.RecipientId)
                .Index(t => t.ChannelAccountModel_Id);
            
            CreateTable(
                "dbo.ConversationAccountModels",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IsGroup = c.Boolean(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChannelAccountModels",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        UserAccountId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountId)
                .Index(t => t.UserAccountId);
            
            CreateTable(
                "dbo.UserAccounts",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Role = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActivityModels", "RecipientId", "dbo.ChannelAccountModels");
            DropForeignKey("dbo.ActivityModels", "FromId", "dbo.ChannelAccountModels");
            DropForeignKey("dbo.ChannelAccountModels", "UserAccountId", "dbo.UserAccounts");
            DropForeignKey("dbo.ActivityModels", "ChannelAccountModel_Id", "dbo.ChannelAccountModels");
            DropForeignKey("dbo.ActivityModels", "ConversationId", "dbo.ConversationAccountModels");
            DropIndex("dbo.ChannelAccountModels", new[] { "UserAccountId" });
            DropIndex("dbo.ActivityModels", new[] { "ChannelAccountModel_Id" });
            DropIndex("dbo.ActivityModels", new[] { "RecipientId" });
            DropIndex("dbo.ActivityModels", new[] { "ConversationId" });
            DropIndex("dbo.ActivityModels", new[] { "FromId" });
            DropIndex("dbo.ActivityModels", new[] { "ActivityId" });
            DropTable("dbo.UserAccounts");
            DropTable("dbo.ChannelAccountModels");
            DropTable("dbo.ConversationAccountModels");
            DropTable("dbo.ActivityModels");
        }
    }
}
