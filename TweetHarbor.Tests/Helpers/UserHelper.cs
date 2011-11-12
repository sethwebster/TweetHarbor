using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetHarbor.Models;
using System.Collections.ObjectModel;

namespace TweetHarbor.Tests.Helpers
{
    class UserHelper
    {
        public static User ArrangeNewUserDefault()
        {
            var user = new User()
            {
                EmailAddress = "sethwebster@gmail.com",
                UniqueId = "db7a3a64156d0b33beae93fe99ca599e",
                SendPrivateTweet = true,
                SendPublicTweet = false,
                SendSMS = true,
                UserName = "localtestuser"
            };

            user.AuthenticationAccounts = new Collection<UserAuthenticationAccount>();
            user.AuthenticationAccounts.Add(new UserAuthenticationAccount()
            {
                AccountProvider = "twitter",
                OAuthToken = Guid.NewGuid().ToString(),
                OAuthTokenSecret = Guid.NewGuid().ToString(),
                User = user,
                UserName = "twitteruser"
            });

            user.AuthenticationAccounts.Add(new UserAuthenticationAccount()
            {
                AccountProvider = "appharbor",
                OAuthToken = Guid.NewGuid().ToString(),
                OAuthTokenSecret = Guid.NewGuid().ToString(),
                User = user,
                UserName = "appharboruser@appharbor.com"
            });
            return user;
        }
    }
}
