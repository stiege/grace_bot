namespace GraceBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyActivityModel : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.ActivityModels", name: "Conversation_Id", newName: "ConversationId");
            RenameColumn(table: "dbo.ActivityModels", name: "From_Id", newName: "FromId");
            RenameColumn(table: "dbo.ActivityModels", name: "Recipient_Id", newName: "RecipientId");
            RenameIndex(table: "dbo.ActivityModels", name: "IX_From_Id", newName: "IX_FromId");
            RenameIndex(table: "dbo.ActivityModels", name: "IX_Conversation_Id", newName: "IX_ConversationId");
            RenameIndex(table: "dbo.ActivityModels", name: "IX_Recipient_Id", newName: "IX_RecipientId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ActivityModels", name: "IX_RecipientId", newName: "IX_Recipient_Id");
            RenameIndex(table: "dbo.ActivityModels", name: "IX_ConversationId", newName: "IX_Conversation_Id");
            RenameIndex(table: "dbo.ActivityModels", name: "IX_FromId", newName: "IX_From_Id");
            RenameColumn(table: "dbo.ActivityModels", name: "RecipientId", newName: "Recipient_Id");
            RenameColumn(table: "dbo.ActivityModels", name: "FromId", newName: "From_Id");
            RenameColumn(table: "dbo.ActivityModels", name: "ConversationId", newName: "Conversation_Id");
        }
    }
}
