namespace TweetHarbor.Models.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddUserId : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "Projects", name: "User_TwitterUserName", newName: "User_UserId");
            AddColumn("Users", "UserId", c => c.Int(nullable: false, identity: true));
            AddForeignKey("Projects", "User_UserId", "Users", "UserId");
            DropForeignKey("Projects", "User_UserId", "Users", "TwitterUserName");
        }
        
        public override void Down()
        {
            AddForeignKey("Projects", "User_UserId", "Users", "TwitterUserName");
            DropForeignKey("Projects", "User_UserId", "Users", "UserId");
            DropColumn("Users", "UserId");
            RenameColumn(table: "Projects", name: "User_UserId", newName: "User_TwitterUserName");
        }
    }
}
