namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExtendedActivities", "ActivityId", c => c.String());
            AddColumn("dbo.ExtendedActivities", "ReplyToId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExtendedActivities", "ReplyToId");
            DropColumn("dbo.ExtendedActivities", "ActivityId");
        }
    }
}
