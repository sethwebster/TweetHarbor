using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TweetHarbor.Models;
using System.Data.Entity;

namespace TweetHarbor.Data
{
    public class LocalDataInitializationStrategy : DropCreateDatabaseIfModelChanges<TweetHarborDbContext>
    {
        protected override void Seed(TweetHarborDbContext context)
        {
            var user = new User()
            {
                TwitterUserName = "localtestuser",
                OAuthToken = Guid.NewGuid().ToString(),
                OAuthTokenSecret = Guid.NewGuid().ToString(),
                UniqueId = Guid.NewGuid().ToString()
            };
            user.Projects.Add(new Project() { ProjectName = "Test Project 1" });
            user.Projects.Add(new Project() { ProjectName = "TestProject2" });
            context.Users.Add(user);
            try
            {
                context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
            base.Seed(context);
        }
    }
}