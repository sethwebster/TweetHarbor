using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetHarbor.Data;
using TweetHarbor.Models;

namespace TweetHarbor.Tests.Helpers
{
    public class TestTweetHarborDbContext : ITweetHarborDbContext
    {
        TestableDbSet<User> users = new TestableDbSet<User>();
        TestableDbSet<OldUser> oldusers = new TestableDbSet<OldUser>();
        TestableDbSet<Project> projects = new TestableDbSet<Project>();
        TestableDbSet<TwitterMessageRecipient> messageRecipients = new TestableDbSet<TwitterMessageRecipient>();
        TestableDbSet<LogEntry> logEntries = new TestableDbSet<LogEntry>();
        TestableDbSet<TextMessageRecipient> textMessageRecipients = new TestableDbSet<TextMessageRecipient>();
        TestableDbSet<ProjectNotification> projectNotifications = new TestableDbSet<ProjectNotification>();
        TestableDbSet<Build> builds = new TestableDbSet<Build>();
        TestableDbSet<Commit> commits = new TestableDbSet<Commit>();
        TestableDbSet<OutboundNotification> outboundNotifications = new TestableDbSet<OutboundNotification>();
        TestableDbSet<UserAuthenticationAccount> userAuthenticationAccounts = new TestableDbSet<UserAuthenticationAccount>();

        public System.Data.Entity.IDbSet<Commit> Commits
        {
            get { return commits; }
            set { commits = (TestableDbSet<Commit>)value; }
        }

        public System.Data.Entity.IDbSet<OutboundNotification> OutboundNotifications
        {
            get { return outboundNotifications; }
            set { outboundNotifications = (TestableDbSet<OutboundNotification>)value; }
        }

        public System.Data.Entity.IDbSet<LogEntry> LogEntries
        {
            get { return logEntries; }
            set { logEntries = (TestableDbSet<LogEntry>)value; }
        }

        public System.Data.Entity.IDbSet<ProjectNotification> ProjectNotifications
        {
            get { return projectNotifications; }
            set { projectNotifications = (TestableDbSet<ProjectNotification>)value; }
        }

        public System.Data.Entity.IDbSet<Build> Builds
        {
            get { return builds; }
            set { builds = (TestableDbSet<Build>)value; }
        }

        public System.Data.Entity.IDbSet<Models.User> Users
        {
            get
            {
                return users;
            }
            set
            {
                users = (TestableDbSet<User>)value;
            }
        }

        public System.Data.Entity.IDbSet<Models.OldUser> OldUsers
        {
            get
            {
                return oldusers;
            }
            set
            {
                oldusers = (TestableDbSet<OldUser>)value;
            }
        }

        public System.Data.Entity.IDbSet<Models.Project> Projects
        {
            get
            {
                return projects;
            }
            set
            {
                projects = (TestableDbSet<Project>)value;
            }
        }

        public System.Data.Entity.IDbSet<Models.TwitterMessageRecipient> MessageRecipients
        {
            get
            {
                return this.messageRecipients;
            }
            set
            {
                this.messageRecipients = (TestableDbSet<TwitterMessageRecipient>)value;
            }
        }
        public System.Data.Entity.IDbSet<Models.TextMessageRecipient> TextMessageRecipients
        {
            get
            {
                return this.textMessageRecipients;
            }
            set
            {
                this.textMessageRecipients = (TestableDbSet<TextMessageRecipient>)value;
            }
        }

        public System.Data.Entity.IDbSet<UserAuthenticationAccount> UserAuthenticationAccounts
        {
            get
            {
                return this.userAuthenticationAccounts;
            }
            set
            {
                this.userAuthenticationAccounts = (TestableDbSet<UserAuthenticationAccount>)value;
            }
        }


        public int SaveChanges()
        {
            return 1;
        }

        public void Dispose()
        {

        }
    }
}
