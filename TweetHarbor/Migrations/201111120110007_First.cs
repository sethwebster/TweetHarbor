namespace TweetHarbor.Models.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class First : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Users",
                c => new
                    {
                        TwitterUserName = c.String(nullable: false, maxLength: 255),
                        OAuthToken = c.String(nullable: false, maxLength: 255),
                        OAuthTokenSecret = c.String(nullable: false, maxLength: 255),
                        UniqueId = c.String(nullable: false, maxLength: 255),
                        EmailAddress = c.String(maxLength: 255),
                        UserProfilePicUrl = c.String(),
                        SendPrivateTweet = c.Boolean(nullable: false),
                        SendPublicTweet = c.Boolean(nullable: false),
                        SendSMS = c.Boolean(nullable: false),
                        IsAdmin = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TwitterUserName);
            
            CreateTable(
                "Projects",
                c => new
                    {
                        ProjectId = c.Int(nullable: false, identity: true),
                        ProjectName = c.String(nullable: false, maxLength: 255),
                        SendPrivateTweetOnSuccess = c.Boolean(nullable: false),
                        SendPublicTweetOnSuccess = c.Boolean(nullable: false),
                        SendPrivateTweetOnFailure = c.Boolean(nullable: false),
                        SendPublicTweetOnFailure = c.Boolean(nullable: false),
                        SendTextOnSuccess = c.Boolean(nullable: false),
                        SendTextOnFailure = c.Boolean(nullable: false),
                        SuccessTemplate = c.String(),
                        FailureTemplate = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        AppHarborProjectUrl = c.String(maxLength: 400),
                        User_TwitterUserName = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.ProjectId)
                .ForeignKey("Users", t => t.User_TwitterUserName);
            
            CreateTable(
                "TwitterMessageRecipients",
                c => new
                    {
                        TwitterMessageRecipientId = c.Int(nullable: false, identity: true),
                        ScreenName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.TwitterMessageRecipientId);
            
            CreateTable(
                "ProjectNotifications",
                c => new
                    {
                        ProjectNotificationId = c.Int(nullable: false, identity: true),
                        NotificationDate = c.DateTime(nullable: false),
                        Project_ProjectId = c.Int(),
                        Build_BuildId = c.Int(),
                    })
                .PrimaryKey(t => t.ProjectNotificationId)
                .ForeignKey("Projects", t => t.Project_ProjectId)
                .ForeignKey("Builds", t => t.Build_BuildId);
            
            CreateTable(
                "Builds",
                c => new
                    {
                        BuildId = c.Int(nullable: false, identity: true),
                        status = c.String(),
                        commit_id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.BuildId)
                .ForeignKey("Commits", t => t.commit_id);
            
            CreateTable(
                "Commits",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 128),
                        message = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "TextMessageRecipients",
                c => new
                    {
                        PhoneNumber = c.String(nullable: false, maxLength: 10),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.PhoneNumber);
            
            CreateTable(
                "OutboundNotifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        NotificationType = c.String(),
                        Recipient = c.String(),
                        Message = c.String(),
                        SentSuccessfully = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        DateSent = c.DateTime(nullable: false),
                        Project_ProjectId = c.Int(),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("Projects", t => t.Project_ProjectId);
            
            CreateTable(
                "LogEntries",
                c => new
                    {
                        LogEntryId = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Message = c.String(),
                        StackTrace = c.String(),
                    })
                .PrimaryKey(t => t.LogEntryId);
            
            CreateTable(
                "EdmMetadata",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ModelHash = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "TwitterMessageRecipientProjects",
                c => new
                    {
                        TwitterMessageRecipient_TwitterMessageRecipientId = c.Int(nullable: false),
                        Project_ProjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TwitterMessageRecipient_TwitterMessageRecipientId, t.Project_ProjectId })
                .ForeignKey("TwitterMessageRecipients", t => t.TwitterMessageRecipient_TwitterMessageRecipientId)
                .ForeignKey("Projects", t => t.Project_ProjectId);
            
            CreateTable(
                "TextMessageRecipientProjects",
                c => new
                    {
                        TextMessageRecipient_PhoneNumber = c.String(nullable: false, maxLength: 10),
                        Project_ProjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TextMessageRecipient_PhoneNumber, t.Project_ProjectId })
                .ForeignKey("TextMessageRecipients", t => t.TextMessageRecipient_PhoneNumber)
                .ForeignKey("Projects", t => t.Project_ProjectId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("TextMessageRecipientProjects", "Project_ProjectId", "Projects", "ProjectId");
            DropForeignKey("TextMessageRecipientProjects", "TextMessageRecipient_PhoneNumber", "TextMessageRecipients", "PhoneNumber");
            DropForeignKey("TwitterMessageRecipientProjects", "Project_ProjectId", "Projects", "ProjectId");
            DropForeignKey("TwitterMessageRecipientProjects", "TwitterMessageRecipient_TwitterMessageRecipientId", "TwitterMessageRecipients", "TwitterMessageRecipientId");
            DropForeignKey("OutboundNotifications", "Project_ProjectId", "Projects", "ProjectId");
            DropForeignKey("Builds", "commit_id", "Commits", "id");
            DropForeignKey("ProjectNotifications", "Build_BuildId", "Builds", "BuildId");
            DropForeignKey("ProjectNotifications", "Project_ProjectId", "Projects", "ProjectId");
            DropForeignKey("Projects", "User_TwitterUserName", "Users", "TwitterUserName");
            DropTable("TextMessageRecipientProjects");
            DropTable("TwitterMessageRecipientProjects");
            DropTable("EdmMetadata");
            DropTable("LogEntries");
            DropTable("OutboundNotifications");
            DropTable("TextMessageRecipients");
            DropTable("Commits");
            DropTable("Builds");
            DropTable("ProjectNotifications");
            DropTable("TwitterMessageRecipients");
            DropTable("Projects");
            DropTable("Users");
        }
    }
}
