using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TweetHarbor.Models;
using System.Data.Entity;
using System.Collections.ObjectModel;

namespace TweetHarbor.Data
{
    public class LocalDataInitializationStrategy : DropCreateDatabaseIfModelChanges<TweetHarborDbContext>
    {
        protected override void Seed(TweetHarborDbContext context)
        {
            var user = new User()
            {
                UserName = "localtestuser",
                EmailAddress = "local@test.com",
                UniqueId = Guid.NewGuid().ToString()
            };
            user.AuthenticationAccounts = new Collection<UserAuthenticationAccount>();
            user.AuthenticationAccounts.Add(new UserAuthenticationAccount()
            {
                AccountProvider = "twitter",
                OAuthToken = Guid.NewGuid().ToString(),
                OAuthTokenSecret = Guid.NewGuid().ToString(),
                UserName = "twutteruser",
                ProfilePicUrl = "http://wwww.pic.com"
            });

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