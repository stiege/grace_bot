namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTwitterQuestionModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TwitterQuestions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Text = c.String(nullable: false),
                        StatusId = c.String(nullable: false),
                        UserScreenName = c.String(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TwitterQuestions");
        }
    }
}
