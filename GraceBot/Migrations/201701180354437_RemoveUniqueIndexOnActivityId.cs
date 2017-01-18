namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUniqueIndexOnActivityId : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ActivityModels", new[] { "ActivityId" });
            AlterColumn("dbo.ActivityModels", "ActivityId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ActivityModels", "ActivityId", c => c.String(maxLength: 64));
            CreateIndex("dbo.ActivityModels", "ActivityId", unique: true);
        }
    }
}
