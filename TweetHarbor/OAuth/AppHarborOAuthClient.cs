using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TweetHarbor.Data;
using TweetHarbor.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace TweetHarbor.OAuth
{
    public class AppHarborOAuthClient : IOAuthSignInClient
    {
        string clientId = string.Empty;
        string secret = string.Empty;
        ITweetHarborDbContext database = null;
        public AppHarborOAuthClient(string clientId, string secret, ITweetHarborDbContext database)
        {
            this.clientId = clientId;
            this.secret = secret;
            this.database = database;
        }

        public string Name
        {
            get { return "appharbor"; }
        }

        public Uri GetAuthenticationEndpoint(string returnToUrl)
        {

            return new Uri(new AppHarbor.Client.AppHarborClient(clientId, secret).GetAuthorizationUrl(returnToUrl).AbsoluteUri);
        }


        public User OAuthCallback(HttpRequestBase Request)
        {
            var client = new AppHarbor.Client.AppHarborClient(clientId, secret);

            var token = client.GetAccessToken(Request["Code"]);
            var user = client.GetUserInformation(token);

            User masterUser = null;
            if (!string.IsNullOrEmpty(Request["Id"]))
            {
                masterUser = database.Users.FirstOrDefault(u => u.UniqueId == Request["Id"]);
            }

            var appUser = AppHarborCreateOrUpdateAccountIfNeeded(token, user, masterUser);
            return appUser;
        }

        private User AppHarborCreateOrUpdateAccountIfNeeded(string AccessToken, AppHarbor.Client.User user, User returnUser)
        {
            //TODO: must have some kind of AppHb unique id-- username, etc --see twitter approach (screenname) (for now we used emailaddress)
            if (null == returnUser)
            {
                returnUser = (from u in database.Users
                              where u.AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "appharbor" && ac.UserName == user.UserName) != null
                              select u).FirstOrDefault();
            }
            if (null == returnUser) // CREATE
            {
                var existingUser = (from u in database.Users where u.UserName.ToLower() == user.UserName.ToLower() select u).FirstOrDefault();
                returnUser = new User();
                returnUser.UserName = null == existingUser ? user.UserName : "";
                returnUser.EmailAddress = user.EmailAddress;
                returnUser.UpdateUniqueId();
                database.Users.Add(returnUser);
            }

            var newAppHarborAccount = returnUser.AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "appharbor" &&
                ac.UserName == user.UserName);

            if (newAppHarborAccount == null)
            {
                newAppHarborAccount = new UserAuthenticationAccount();
                newAppHarborAccount.AccountProvider = "appharbor";
                newAppHarborAccount.UserName = user.UserName;
                newAppHarborAccount.UserName = user.UserName;
                newAppHarborAccount.ProfilePicUrl = "<not implemented>";
                if (null == returnUser.AuthenticationAccounts)
                    returnUser.AuthenticationAccounts = new Collection<UserAuthenticationAccount>();
                returnUser.AuthenticationAccounts.Add(newAppHarborAccount);
            }

            //returnUser.UserProfilePicUrl = user.ProfileImageUrl;
            var appharborAccount = returnUser.AuthenticationAccounts.First(t => t.AccountProvider == "appharbor");
            appharborAccount.OAuthToken = AccessToken;
            //appharborAccount.OAuthTokenSecret = accessToken.TokenSecret;
            //appharborAccount.ProfilePicUrl = user.ProfileImageUrl;

            try
            {
                database.SaveChanges();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception: " + e.Message);
                throw e;
            }
            return returnUser;
        }
    }
}