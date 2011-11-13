using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using TweetHarbor.Data;

namespace TweetHarbor.Models
{
    public class TweetHarborDbContext : DbContext, ITweetHarborDbContext
    {

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

        public TweetHarborDbContext() { }
        public virtual IDbSet<User> Users { get; set; }
        public virtual IDbSet<OldUser> OldUsers { get; set; }
        public virtual IDbSet<Project> Projects { get; set; }
        public virtual IDbSet<TwitterMessageRecipient> MessageRecipients { get; set; }
        public virtual IDbSet<TextMessageRecipient> TextMessageRecipients { get; set; }
        public virtual IDbSet<LogEntry> LogEntries { get; set; }
        public virtual IDbSet<ProjectNotification> ProjectNotifications { get; set; }
        public virtual IDbSet<Build> Builds { get; set; }
        public virtual IDbSet<Commit> Commits { get; set; }
        public virtual IDbSet<OutboundNotification> OutboundNotifications { get; set; }
        public virtual IDbSet<UserAuthenticationAccount> UserAuthenticationAccounts { get; set; }

        public void Dispose()
        {
            base.Dispose();
        }
    }
}