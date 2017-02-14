namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAnswerRating : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ActivityModels", "IX_ActivityId");

            CreateTable(
                "dbo.AnswerRatings",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Rate = c.Int(nullable: false),
                        AnswerId = c.String(nullable: false, maxLength: 128),
                        AnswerActivityId = c.String(nullable: false, maxLength: 128),
                        RaterChannelAccountId = c.String(nullable: false, maxLength: 128),
                        CommentActivityId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Answers", t => t.AnswerId, cascadeDelete: true)
                .ForeignKey("dbo.ActivityModels", t => t.AnswerActivityId, cascadeDelete: true)
                .ForeignKey("dbo.ActivityModels", t => t.CommentActivityId)
                .ForeignKey("dbo.ChannelAccountModels", t => t.RaterChannelAccountId, cascadeDelete: true)
                .Index(t => t.AnswerId)
                .Index(t => t.AnswerActivityId)
                .Index(t => t.RaterChannelAccountId)
                .Index(t => t.CommentActivityId);
            
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Subject = c.String(nullable: false),
                        Text = c.String(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        Remarks = c.String(),
                        AuthorId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserAccounts", t => t.AuthorId)
                .Index(t => t.AuthorId);
            
            DropColumn("dbo.ActivityModels", "ActivityId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActivityModels", "ActivityId", c => c.String());
            DropForeignKey("dbo.AnswerRatings", "RaterChannelAccountId", "dbo.ChannelAccountModels");
            DropForeignKey("dbo.AnswerRatings", "CommentActivityId", "dbo.ActivityModels");
            DropForeignKey("dbo.AnswerRatings", "AnswerActivityId", "dbo.ActivityModels");
            DropForeignKey("dbo.AnswerRatings", "AnswerId", "dbo.Answers");
            DropForeignKey("dbo.Answers", "AuthorId", "dbo.UserAccounts");
            DropIndex("dbo.Answers", new[] { "AuthorId" });
            DropIndex("dbo.AnswerRatings", new[] { "CommentActivityId" });
            DropIndex("dbo.AnswerRatings", new[] { "RaterChannelAccountId" });
            DropIndex("dbo.AnswerRatings", new[] { "AnswerActivityId" });
            DropIndex("dbo.AnswerRatings", new[] { "AnswerId" });
            DropTable("dbo.Answers");
            DropTable("dbo.AnswerRatings");
        }
    }
}
