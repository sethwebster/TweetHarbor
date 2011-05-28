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
        TestableDbSet<Project> projects = new TestableDbSet<Project>();

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

        public int SaveChanges()
        {
            return 1;   
        }

        public void Dispose()
        {
            
        }
    }
}
