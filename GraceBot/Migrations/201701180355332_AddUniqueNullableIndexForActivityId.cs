namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddUniqueNullableIndexForActivityId : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ActivityModels", "ActivityId", c => c.String(maxLength: 64));
            Sql(string.Format(@"
                CREATE UNIQUE NONCLUSTERED INDEX {0}
                ON {1}({2}) 
                WHERE {2} IS NOT NULL;",
                "IX_ActivityId", "dbo.ActivityModels", "ActivityId"));
        }

        public override void Down()
        {
            DropIndex("dbo.ActivityModels", "IX_ActivityId");
            AlterColumn("dbo.ActivityModels", "ActivityId", c => c.String());
        }
    }
}
