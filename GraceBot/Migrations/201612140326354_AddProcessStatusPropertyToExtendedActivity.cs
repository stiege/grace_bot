namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProcessStatusPropertyToExtendedActivity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExtendedActivities", "ProcessStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExtendedActivities", "ProcessStatus");
        }
    }
}
