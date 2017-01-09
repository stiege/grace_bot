namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyModels : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ExtendedActivities", newName: "ActivityModels");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ActivityModels", newName: "ExtendedActivities");
        }
    }
}
